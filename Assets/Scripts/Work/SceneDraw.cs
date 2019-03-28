using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Project.Work
{
    public class SceneDraw : MonoBehaviour
    {
        public Image DrawPanel;
        [Range(1, 3)]
        public int PolygonLineWide = 2;
        public Color PolygonLineColor;
        [Range(1, 3)]
        public int VisibleGLineWide = 1;
        public Color VisibleGLineColor;
        [Range(1, 3)]
        public int ResultPathLineWide = 3;
        public Color ResultPathLineColor;

        private Texture2D _texture;
        private Rect _textureRect;

        [Inject] private readonly IDrawManager _drawManager;

        public void DrawScene(PolygonScene scene)
        {
            Init();
        }

        private void Init()
        {
            _texture = new Texture2D((int)DrawPanel.rectTransform.rect.width, (int)DrawPanel.rectTransform.rect.height);
            DrawPanel.material.SetTexture("_MainTex", _texture);
            _drawManager.SetCurrentDrawTexture(_texture);
            SetTextureRect();
        }

        private void SetTextureRect()
        {
            Vector3[] fourCornersArray = new Vector3[4];
            DrawPanel.rectTransform.GetWorldCorners(fourCornersArray);
            fourCornersArray[0] = Camera.main.WorldToScreenPoint(fourCornersArray[0]);
            fourCornersArray[2] = Camera.main.WorldToScreenPoint(fourCornersArray[2]);
            _textureRect = new Rect();
            _textureRect.min = new Vector2(fourCornersArray[0].x, fourCornersArray[0].y);
            _textureRect.max = new Vector2(fourCornersArray[2].x, fourCornersArray[2].y);
        }

        //private void OnGUI()
        //{
        //    GUIStyle style = new GUIStyle();
        //    style.fontSize = 30;
        //    GUI.Label(new Rect(20, 20, 150, 80), "OutTexture: " + OutTexture(_mousePoint), style);
        //    GUI.Label(new Rect(60, 60, 150, 80), "LineColor: " + LineColor, style);
        //    //GUI.Label(new Rect(200, 200, 150, 80), "Rect : " + DrawPanel.rectTransform.rect.size, style);
        //}
    }

}

