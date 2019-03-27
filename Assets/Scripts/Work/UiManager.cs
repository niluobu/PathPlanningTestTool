using UnityEngine;
using UnityEngine.UI;

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

        private void Start()
        {
            InitView();
            AddButtonListener();
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
        }

        private void ShowMainPanel()
        {
            MainPanel.SetActive(true);
            EditPanel.SetActive(false);
            RunPanel.SetActive(false);
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
