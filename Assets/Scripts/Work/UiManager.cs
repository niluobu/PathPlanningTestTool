using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Project.Work
{
    public class UiManager : MonoBehaviour
    {
        public SceneEditor SceneEditor;
        public SceneDraw SceneDraw;
        [Header("-------------")]
        public GameObject PopupPanel;
        public Text TipsText;
        public Button OkButton;
        public Button CancelButton;
        [Header("-------------")]
        public GameObject MainPanel;
        public Button CreateButton;
        public GameObject EmptyText;
        public Transform SceneItemRoot;
        public GameObject SceneItemPrefab;
        [Header("-------------")]
        public GameObject EditPanel;
        public Button UndoButton;
        public Button SaveButton;
        public Button ExitEditButton;
        [Header("-------------")]
        public GameObject RunPanel;
        public Toggle VisibleToggle;
        public Button StartButton;
        public Button ResetButton;
        public Button ExitRunButton;
        public GameObject TextPanel;
        public Text VertexNumText;
        public Text EdgeNumText;
        public Text CVGTimeText;
        public Text SPTimeText;
        public Text HintText;

        [Inject] private readonly IPolygonSceneStorer _polygonStorer;

        private readonly Subject<bool> _confirmSubject = new Subject<bool>();
        private IObservable<bool> _confirmAsObservable => _confirmSubject;

        private void Start()
        {
            InitView();
            AddButtonListener();
            AddSubscribe();
        }

        private void InitView()
        {
            ShowMainPanel();
            RefreshSceneItem();
            PopupPanel.SetActive(false);
            SceneEditor.EditorOn = false;
        }

        private void AddButtonListener()
        {
            CreateButton.onClick.AddListener(() =>
            {
                SceneEditor.EditorOn = true;
                ShowEditPanel();
            });

            SaveButton.onClick.AddListener(async () =>
            {
                SceneEditor.EditorOn = false;
                bool succeed = SceneEditor.SaveScene();
                if (!succeed)
                {
                    ShowPopupPanel("场景中有无效的多边形，程序已自动将它们删除，请再次保存或继续编辑场景", false);
                }
                else
                {
                    ShowPopupPanel("保存成功！\n退回上一级可查看该新场景的信息，并选择进行测试", false);
                }
                await _confirmAsObservable;
                SceneEditor.EditorOn = true;
            });

            ExitEditButton.onClick.AddListener(async () =>
            {
                SceneEditor.EditorOn = false;
                if (!SceneEditor.SceneDirty)
                {
                    ShowMainPanel();
                    SceneEditor.ClearScene();
                }
                else
                {
                    ShowPopupPanel("是否退出？（当前场景的编辑进度将会丢失）", true);
                    bool chioce = await _confirmAsObservable;
                    if (chioce == true)
                    {
                        ShowMainPanel();
                        SceneEditor.ClearScene();
                    }
                    else
                    {
                        SceneEditor.EditorOn = true;
                    }
                }
            });

            UndoButton.onClick.AddListener(async () =>
            {
                SceneEditor.EditorOn = false;
                bool succeed = SceneEditor.Undo();
                if (!succeed)
                {
                    ShowPopupPanel("只支持撤销当前编辑的多边形操作！", false);
                    await _confirmAsObservable;
                }
                SceneEditor.EditorOn = true;
            });

            OkButton.onClick.AddListener(() =>
            {
                _confirmSubject.OnNext(true);
                PopupPanel.SetActive(false);
            });

            CancelButton.onClick.AddListener(() =>
            {
                _confirmSubject.OnNext(false);
                PopupPanel.SetActive(false);
            });

            ExitRunButton.onClick.AddListener(() =>
            {
                ShowMainPanel();
                SceneDraw.ClearDrawPanel();
            });

            StartButton.onClick.AddListener(async () =>
            {

            });

            ResetButton.onClick.AddListener(async () =>
            {

            });

            VisibleToggle.onValueChanged.AddListener(x =>
            {

            });
        }

        private void AddSubscribe()
        {
            _polygonStorer.AddPolygonScenesAsObservable
                .Subscribe(AddSceneItem);
        }

        private void ShowMainPanel()
        {
            MainPanel.SetActive(true);
            EditPanel.SetActive(false);
            RunPanel.SetActive(false);
        }

        private void RefreshSceneItem()
        {
            EmptyText.gameObject.SetActive(_polygonStorer.PolygonScenes.Count == 0);
            foreach (var scene in _polygonStorer.PolygonScenes)
            {
                AddSceneItem(scene);
            }
        }

        private void AddSceneItem(PolygonScene scene)
        {
            GameObject item = GameObject.Instantiate(SceneItemPrefab);
            item.transform.SetParent(SceneItemRoot, false);
            item.name = $"scene No. {scene.SceneNum}";
            Text numText = item.GetComponentInChildren<Text>();
            numText.text = item.name;
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.AddListener(() =>
            {
                SceneDraw.DrawScene(scene);
                ShowRunPanel();
            });
        }

        private void ShowEditPanel()
        {
            MainPanel.SetActive(false);
            EditPanel.SetActive(true);
            RunPanel.SetActive(false);
        }

        private void ShowRunPanel()
        {
            MainPanel.SetActive(false);
            EditPanel.SetActive(false);
            RunPanel.SetActive(true);
            SetTextPanel(false);
            ResetParameterText();
        }

        private void ResetParameterText()
        {
            string emptyText = "_ _";
            VertexNumText.text = emptyText;
            EdgeNumText.text = emptyText;
            CVGTimeText.text = emptyText;
            SPTimeText.text = emptyText;
        }

        private void SetTextPanel(bool visible)
        {
            HintText.gameObject.SetActive(!visible);
            TextPanel.SetActive(visible);
        }

        private void ShowPopupPanel(string tip, bool doChoice)
        {
            PopupPanel.SetActive(true);
            TipsText.text = tip;
            CancelButton.gameObject.SetActive(doChoice);
        }
    }

}
