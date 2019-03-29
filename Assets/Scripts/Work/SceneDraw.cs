using System.Collections.Generic;
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
        public Color FillColor;
        public Color TextureBgColor;

        private Texture2D _texture;
        private Rect _textureRect;

        [Inject] private readonly IGlUtil _glUtil;

        public void DrawScene(PolygonScene scene)
        {
            InitTexture();
            DrawPolygon(scene);
            Apply();
        }

        public void ClearDrawPanel()
        {
            SetTextureBgColor();
        }

        private void Apply()
        {
            _texture.Apply();
            DrawPanel.enabled = false;
            DrawPanel.enabled = true;
        }

        private void InitTexture()
        {
            if (DrawPanel.material.mainTexture == null)
            {
                _texture = new Texture2D((int)DrawPanel.rectTransform.rect.width, (int)DrawPanel.rectTransform.rect.height);
                DrawPanel.material.SetTexture("_MainTex", _texture);
                SetTextureBgColor();
                SetTextureRect();
            }
        }

        private void SetTextureBgColor()
        {
            for (int mip = 0; mip < _texture.mipmapCount; ++mip)
            {
                Color[] cols = _texture.GetPixels(mip);
                for (int i = 0; i < cols.Length; ++i)
                {
                    cols[i] = TextureBgColor;
                }
                _texture.SetPixels(cols, mip);
            }

            Apply();
        }

        private void DrawPolygon(PolygonScene scene)
        {
            scene.Polygons.Sort((a, b) => a.GetMinX() - b.GetMinX());
            List<int> insidePolygonNums = new List<int>();
            foreach (Polygon polygon in scene.Polygons)
            {
                //for (int i = 0; i < polygon.Vertexes.Count; ++i)
                //{
                //    DrawLine(polygon.Vertexes[i].Position, polygon.Vertexes[(i + 1) % polygon.Vertexes.Count].Position,
                //        PolygonLineColor, PolygonLineWide);
                //}
                if (!_glUtil.FillPolygon(polygon, FillColor, _texture, ConvertPosition))
                {
                    insidePolygonNums.Add(polygon.PolygonNum);
                }
            }

            foreach (var insidePolygonNum in insidePolygonNums)
            {
                Polygon insidePolygon = scene.Polygons.Find(x => x.PolygonNum == insidePolygonNum);
                if (insidePolygon != null)
                {
                    scene.Polygons.Remove(insidePolygon);
                }
            }
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

        private void DrawLine(Vector2Int startPoint, Vector2Int endPoint, Color lineColor, int lineWide)
        {
            startPoint = ConvertPosition(startPoint);
            endPoint = ConvertPosition(endPoint);
            _glUtil.DrawLine(startPoint, endPoint, lineColor, _texture);
            for (int i = 1; i < lineWide; ++i)
            {
                if (Mathf.Abs(startPoint.x - endPoint.x) < Mathf.Abs(startPoint.y - endPoint.y) || startPoint.x == endPoint.x)
                {
                    _glUtil.DrawLine(new Vector2Int(startPoint.x + i, startPoint.y), new Vector2Int(endPoint.x + i, endPoint.y),
                        lineColor, _texture);
                    _glUtil.DrawLine(new Vector2Int(startPoint.x - i, startPoint.y), new Vector2Int(endPoint.x - i, endPoint.y),
                        lineColor, _texture);
                }
                else
                {
                    _glUtil.DrawLine(new Vector2Int(startPoint.x, startPoint.y + i), new Vector2Int(endPoint.x, endPoint.y + i),
                        lineColor, _texture);
                    _glUtil.DrawLine(new Vector2Int(startPoint.x, startPoint.y - i), new Vector2Int(endPoint.x, endPoint.y - i),
                        lineColor, _texture);
                }
            }
        }

        private Vector2Int ConvertPosition(Vector2Int pos)
        {
            Vector2Int cpos = new Vector2Int
            {
                x = pos.x - (int)_textureRect.xMin,
                y = pos.y - (int)_textureRect.yMin
            };
            return cpos;
        }
    }

}

