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
        [Header("-------------")]
        public GameObject PopupPanel;
        public Text TipsText;
        public Button OkButton;
        public Button CancelButton;
        [Header("-------------")]
        public GameObject MainPanel;
        public Button CreateButton;
        public Transform SceneItemRoot;
        public GameObject SceneItemPrefab;
        [Header("-------------")]
        public GameObject EditPanel;
        public Button UndoButton;
        public Button SaveButton;
        public Button ExitEditButton;
        [Header("-------------")]
        public GameObject RunPanel;
        public Button StartButton;
        public Button SetButton;
        public Button ExitRunButton;
        public Image ButtonMask;
        public GameObject TextPanel;
        public Text VertexNumText;
        public Text EdgeNumText;
        public Text PolygonEdgeNumText;
        public Text HintText;

        [Inject] private readonly IPolygonSceneStorer _polygonStorer;
        [Inject] private readonly IRunManager _runManager;

        private readonly Subject<bool> _confirmSubject = new Subject<bool>();
        private IObservable<bool> _confirmAsObservable => _confirmSubject;

        private bool _currentSceneHadTested = false;

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
                SceneEditor.IntoSceneEdit();
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
                    ExitEditPanel();
                }
                else
                {
                    ShowPopupPanel("是否退出？（当前场景的编辑进度将会丢失）", true);
                    bool chioce = await _confirmAsObservable;
                    if (chioce == true)
                    {
                        ExitEditPanel();
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
                _runManager.ClearDrawPanel();
                SceneEditor.ExitPointEdit();
            });

            StartButton.onClick.AddListener(async () =>
            {
                if (_currentSceneHadTested)
                {
                    ShowPopupPanel("请设置新的起点和终点，再进行测试！", false);
                    return;
                }
                if (SceneEditor.BeReady())
                {
                    SetHintText("测试程序运行中...");
                    SetButtomMask(true);
                    _runManager.StartTest(SceneEditor.TargetPoints[0], SceneEditor.TargetPoints[1]);
                    _currentSceneHadTested = true;
                }
                else
                {
                    ShowPopupPanel("设置起点和终点后才能开始测试！", false);
                }
            });

            SetButton.onClick.AddListener(async () =>
            {
                if (_currentSceneHadTested)
                {
                    _runManager.ClearDrawPanel();
                    _runManager.DrawScene();
                    ResetParameterText();
                    _currentSceneHadTested = false;
                }
                SceneEditor.SetOrReset();
            });

        }

        private void AddSubscribe()
        {
            _polygonStorer.AddPolygonScenesAsObservable
                .Subscribe(AddSceneItem);

            _runManager.TestStageHintAsObservable
                .Subscribe(SetHintText);

            _runManager.TestEndAsObservable
                .Subscribe(_ =>
                {
                    ShowPopupPanel("测试结束，红线标识了最短路径！", false);
                    SetButtomMask(false);
                    SetParameterText();
                });
        }

        private void ExitEditPanel()
        {
            SceneEditor.ExitSceneEdit();
            ShowMainPanel();
        }

        private void SetButtomMask(bool visible)
        {
            ButtonMask.gameObject.SetActive(visible);
        }

        private void ShowMainPanel()
        {
            MainPanel.SetActive(true);
            EditPanel.SetActive(false);
            RunPanel.SetActive(false);
        }

        private void RefreshSceneItem()
        {
            foreach (var scene in _polygonStorer.PolygonScenes)
            {
                AddSceneItem(scene);
            }
        }

        private void AddSceneItem(PolygonScene scene)
        {
            GameObject item = GameObject.Instantiate(SceneItemPrefab);
            item.transform.SetParent(SceneItemRoot, false);
            item.name = $"S No. {scene.SceneNum}";
            Text numText = item.GetComponentInChildren<Text>();
            numText.text = item.name;
            Button useButton = item.GetComponentInChildren<Button>();
            useButton.onClick.AddListener(() =>
            {
                _runManager.SetTestScene(scene);
                _runManager.DrawScene();
                SceneEditor.IntoPointEdit();
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
            SetParameterTextVisible(false);
            SetButtomMask(false);
            SetHintText("设置起点和终点后，便可开始测试");
            ResetParameterText();
        }

        private void SetParameterText()
        {
            SetParameterTextVisible(true);
            _runManager.GetTestParameter(out int vertexNum, out int edgeNum, out int polygonEdgeNum);
            VertexNumText.text = vertexNum.ToString();
            EdgeNumText.text = edgeNum.ToString();
            PolygonEdgeNumText.text = polygonEdgeNum.ToString();
        }

        private void ResetParameterText()
        {
            string emptyText = "_ _";
            VertexNumText.text = emptyText;
            EdgeNumText.text = emptyText;
            PolygonEdgeNumText.text = emptyText;
        }

        private void SetHintText(string hint)
        {
            HintText.text = hint;
        }

        private void SetParameterTextVisible(bool visible)
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
