using ModestTree;
using Project.Work;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Project.Test
{
    public class PaintUi : MonoBehaviour
    {
        public class InputVertex
        {
            public Vector2Int Pos;
            public bool IncidentEdgeIsDraw = false;
        }

        public Image DrawPanel;
        [Range(1, 3)]
        public int PainterWide = 1;
        [Range(10, 40)]
        public int StartVertexCircleRadius = 10;
        public Color LineColor;
        public Color CircleColor;

        private Texture2D _texture;
        private List<InputVertex> _vertexes = new List<InputVertex>();
        private Vector2Int _mousePoint;
        private Rect _textureRect;

        [Inject] private readonly IDrawManager _drawManager;
        [Inject] private readonly IPolygonStorer _polygonStorer;

        [HideInInspector]
        public bool Pause = false;

        public void ClearPaintTexture()
        {
            _vertexes.Clear();
            _drawManager.ClearPaintTexture();
        }

        private void Start()
        {
            _texture = new Texture2D((int)DrawPanel.rectTransform.rect.width, (int)DrawPanel.rectTransform.rect.height);

            DrawPanel.material.SetTexture("_MainTex", _texture);
            _drawManager.SetCurrentDrawTexture(_texture);
            SetTextureRect();
            Pause = true;
        }

        private void Update()
        {
            if (!Pause)
            {
                _mousePoint = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
                if (!OutTexture(_mousePoint))
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        _vertexes.Add(new InputVertex()
                        {
                            Pos = _mousePoint,
                            IncidentEdgeIsDraw = false
                        });
                    }

                    DrawLine();
                }
            }
        }

        private void DrawLine()
        {
            if (_vertexes.IsEmpty())
            {
                return;
            }

            int lastVertexIndex = _vertexes.Count - 1;
            if (lastVertexIndex == 0 && _vertexes[0].IncidentEdgeIsDraw == false)
            {
                _vertexes[0].IncidentEdgeIsDraw = true;
                _drawManager.DrawCircle(ConvertPosition(_vertexes[0].Pos), StartVertexCircleRadius, CircleColor);
            }
            if (_vertexes[lastVertexIndex].IncidentEdgeIsDraw || lastVertexIndex == 0)
            {
                if (_vertexes.Count > 2 && PosIsCloseToStartVertex(_mousePoint))
                {
                    _drawManager.DrawLine(ConvertPosition(_vertexes[lastVertexIndex].Pos), ConvertPosition(_vertexes[0].Pos),
                        LineColor, PainterWide, isEdge: false);
                }
                else
                {
                    _drawManager.DrawLine(ConvertPosition(_vertexes[lastVertexIndex].Pos), ConvertPosition(_mousePoint),
                        LineColor, PainterWide, isEdge: false);
                }
            }
            else
            {
                if (_vertexes.Count > 2 && PosIsCloseToStartVertex(_vertexes[lastVertexIndex].Pos))
                {
                    EndPolygonPainting();
                    return;
                }
                _drawManager.DrawLine(ConvertPosition(_vertexes[lastVertexIndex - 1].Pos), ConvertPosition(_vertexes[lastVertexIndex].Pos),
                    LineColor, PainterWide, isEdge: true);
                _vertexes[lastVertexIndex].IncidentEdgeIsDraw = true;
            }
        }

        private void EndPolygonPainting()
        {
            int lastVertexIndex = _vertexes.Count - 1;
            _vertexes.RemoveAt(lastVertexIndex);
            lastVertexIndex--;
            _drawManager.DrawLine(ConvertPosition(_vertexes[lastVertexIndex].Pos), ConvertPosition(_vertexes[0].Pos),
                LineColor, PainterWide, isEdge: true);
            StorePolygon();
            _vertexes.Clear();
        }

        private void StorePolygon()
        {
            List<Vector2Int> polygon = new List<Vector2Int>();
            foreach (var vertex in _vertexes)
            {
                polygon.Add(vertex.Pos);
            }
            _drawManager.DrawVertexCircle(polygon);
            //_polygonStorer.StorePolygon(polygon);
        }

        private bool PosIsCloseToStartVertex(Vector2Int pos)
        {
            return Vector2Int.Distance(_vertexes[0].Pos, pos) < StartVertexCircleRadius;
        }

        private Vector2Int ConvertPosition(Vector2Int pos)
        {
            Vector2Int cpos = new Vector2Int();
            cpos.x = pos.x - (int)_textureRect.xMin;
            cpos.y = pos.y - (int)_textureRect.yMin;
            return cpos;
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

        private bool OutTexture(Vector2Int mousePos)
        {
            if (mousePos.x < _textureRect.xMin || mousePos.x > _textureRect.xMax ||
                mousePos.y < _textureRect.yMin || mousePos.y > _textureRect.yMax)
            {
                return true;
            }

            return false;
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

