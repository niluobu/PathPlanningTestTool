using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Project.Work
{
    public class UiManager : MonoBehaviour
    {
        public GameObject MainPanel;
        public Button CreateNewButton;
        public Button SelectOneButton;

        public GameObject EditModelPanel;
        public Button StartEditButton;
        public Button SaveButton;
        public Button PauseButton;
        public Text PauseButtonText;
        public Button ReturnButton;

        public Image MaskImage;
        public Text HintText;

        public GameObject PopupPanel;
        public GameObject TipPanel;
        public Text TipText;
        public Button OkButton;
        public GameObject ConfirmPanel;
        public Text ConfirmText;
        public Button ConfirmButton;
        public Button CancelButton;

        public PaintUi DrawPanel;

        [Inject] private readonly IPolygonStorer _polygonStorer;
        private Status _status = Status.Main;

        private enum Status
        {
            Main,
            Edit,
            Test
        }

        private void Start()
        {
            InitView();
            InitButtonClickEvent();

        }

        private void InitView()
        {
            SwitchToMainPanel();
        }

        private void SwitchToMainPanel()
        {
            PopupPanel.SetActive(false);
            SetMaskImageVisible(true, "Path planning test tool\n                                     —<size=35>made with xueming</size>");
            EditModelPanel.SetActive(false);
            MainPanel.SetActive(true);
            _status = Status.Main;
        }

        private void SwitchToEditPanel()
        {
            PopupPanel.SetActive(false);
            SetMaskImageVisible(true, "press down EDIT button to start edit polygon scene!");
            EditModelPanel.SetActive(true);
            MainPanel.SetActive(false);
            SetPauseButtonText(isPause: false);
        }

        private void SetPauseButtonText(bool isPause)
        {
            PauseButtonText.text = isPause ? "CONTINUE" : "PAUSE";
        }

        private void InitButtonClickEvent()
        {
            CreateNewButton.onClick.AddListener(() =>
            {
                SwitchToEditPanel();
            });

            SelectOneButton.onClick.AddListener(() =>
            {
                //
            });

            StartEditButton.onClick.AddListener(() =>
            {
                if (_status != Status.Edit)
                {
                    DrawPanel.Pause = false;
                    SetMaskImageVisible(false, "");
                    _status = Status.Edit;
                }
            });

            SaveButton.onClick.AddListener(async () =>
            {
                if (_status == Status.Edit)
                {
                    DrawPanel.Pause = true;
                    _polygonStorer.SavePolygonScene();
                    SetMaskImageVisible(true, "wait save polygon scene...");
                    bool result = await _polygonStorer.EndSaveSceneAsObservable;
                    if (result)
                    {
                        _status = Status.Main;
                        SetPopupPanelVisible(true, true, "Save polygon scene succeed!");
                    }
                    else
                    {
                        SetPopupPanelVisible(true, true, "Some polygons are invalid, the program has automatically deleted them, " +
                                                   "please continue to improve the scene.");
                    }
                }
            });

            PauseButton.onClick.AddListener(() =>
            {
                if (_status == Status.Edit)
                {
                    DrawPanel.Pause = !DrawPanel.Pause;
                    SetPauseButtonText(DrawPanel.Pause);
                    if (DrawPanel.Pause)
                    {
                        SetMaskImageVisible(true, "press down CONTINUE button to continue");
                    }
                    else
                    {
                        SetMaskImageVisible(false, "");
                    }
                }
            });

            ReturnButton.onClick.AddListener(() =>
            {
                //
            });

            OkButton.onClick.AddListener(() =>
            {
                SetPopupPanelVisible(false);
                if (_status == Status.Edit)
                {
                    DrawPanel.Pause = false;
                }
                else
                {
                    SwitchToEditPanel();
                }
            });
        }



        private void SetPopupPanelVisible(bool visible, bool isTip = true, string context = "")
        {
            PopupPanel.SetActive(visible);
            TipPanel.SetActive(PopupPanel.activeInHierarchy && isTip);
            TipText.text = context;
            ConfirmPanel.SetActive(PopupPanel.activeInHierarchy && !isTip);
            ConfirmText.text = context;
        }

        private void SetMaskImageVisible(bool visible, string text)
        {
            MaskImage.gameObject.SetActive(visible);
            HintText.text = text;
        }

    }

}

