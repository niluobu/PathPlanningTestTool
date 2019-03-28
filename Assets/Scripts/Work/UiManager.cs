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

        private void Start()
        {
            InitView();
            AddButtonListener();
            AddSubscribe();
        }

        private void InitView()
        {
            ShowMainPanel();
            PopupPanel.SetActive(false);
            SceneEditor.EditorOn = false;
        }

        private void AddButtonListener()
        {
            CreateButton.onClick.AddListener(() =>
            {
                ShowEditPanel();
                SceneEditor.EditorOn = true;
            });

            UndoButton.onClick.AddListener(() =>
            {
                bool succeed = SceneEditor.Undo();
                if (!succeed)
                {
                    Debug.Log("只支持撤销当前编辑的多边形操作！");
                }
            });

            SaveButton.onClick.AddListener(() =>
            {
                bool succeed = SceneEditor.SaveScene();
                if (!succeed)
                {
                    Debug.Log("场景中有无效的多边形，程序已自动将它们删除，请再次保存或继续编辑场景");
                }
                else
                {
                    Debug.Log("保存成功，退回上一级可查看该新场景的信息，并选择进行测试");
                }
            });

            ExitEditButton.onClick.AddListener(() =>
            {
                if (!SceneEditor.SceneDirty)
                {
                    Debug.Log("是否保存场景并退出？");
                    //todo
                }
                else
                {
                    Debug.Log("是否退出（将会舍弃当前未编辑完的多边形）");
                    SceneEditor.EditorOn = false;
                    ShowMainPanel();
                }
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
            RefreshSceneItem();
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
            item.transform.parent = SceneItemRoot;
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
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
        }

        private void SetTextPanel(bool visible)
        {
            HintText.gameObject.SetActive(!visible);
            TextPanel.SetActive(visible);
        }

        private void ShowPopupPanel(string tip)
        {
            PopupPanel.SetActive(true);
            TipsText.text = tip;
        }
    }

}
