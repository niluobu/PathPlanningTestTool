using ModestTree;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    public interface IDrawManager
    {
        void SetCurrentDrawTexture(Texture2D texture);
        void DrawLine(Vector2Int startPoint, Vector2Int endPoint, Color lineColor, int lineWide, bool isEdge);
        void DrawCircle(Vector2Int center, int radius, Color circleColor, bool needClear = true);
        void DrawVertexCircle(List<Vector2Int> polygon);
        void ClearPaintTexture();
    }

    public class DrawManager : IDrawManager
    {
        public class OldPoint
        {
            public Vector2Int Pos;
            public Color OldColor;
            public Color CurrentColor;
        }

        private Texture2D _texture;
        private Color _textureBgColor;
        private List<OldPoint> _tempLinePoints = new List<OldPoint>();
        private List<OldPoint> _tempCirclePoints = new List<OldPoint>();
        private int _vertexRadius = 6;

        public void SetCurrentDrawTexture(Texture2D texture)
        {
            _texture = texture;
            _textureBgColor = _texture.GetPixel(_texture.width / 2, _texture.height / 2);
        }

        public void ClearPaintTexture()
        {
            for (int x = 0; x <= _texture.width; ++x)
            {
                for (int y = 0; y < _texture.height; ++y)
                {
                    DrawPixel(x, y, _textureBgColor, null);
                }
            }
        }

        #region DrawLine
        public void DrawLine(Vector2Int startPoint, Vector2Int endPoint, Color lineColor, int lineWide, bool isEdge)
        {
            if (!_tempLinePoints.IsEmpty())
            {
                ClearTempPoints();
            }
            DrawLine(startPoint.x, startPoint.y, endPoint.x, endPoint.y, lineColor);
            for (int i = 1; i < lineWide; ++i)
            {
                if (Mathf.Abs(startPoint.x - endPoint.x) < Mathf.Abs(startPoint.y - endPoint.y) || startPoint.x == endPoint.x)
                {
                    DrawLine(startPoint.x + i, startPoint.y, endPoint.x + i, endPoint.y, lineColor);
                    DrawLine(startPoint.x - i, startPoint.y, endPoint.x - i, endPoint.y, lineColor);
                }
                else
                {
                    DrawLine(startPoint.x, startPoint.y + i, endPoint.x, endPoint.y + i, lineColor);
                    DrawLine(startPoint.x, startPoint.y - i, endPoint.x, endPoint.y - i, lineColor);
                }
            }
            _texture.Apply();
            if (isEdge)
            {
                _tempLinePoints.Clear();
            }
        }

        private void DrawLine(int x0, int y0, int x1, int y1, Color lineColor)
        {
            bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            int dx = x1 - x0;
            int dy = Mathf.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = y0 < y1 ? 1 : -1;
            for (int x = x0, y = y0; x <= x1; x++)
            {
                if (steep)
                {

                    DrawPixel(y, x, lineColor, _tempLinePoints);
                }
                else
                {
                    DrawPixel(x, y, lineColor, _tempLinePoints);
                }

                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
        #endregion

        #region DrawCircle
        public void DrawCircle(Vector2Int center, int radius, Color circleColor, bool needClear = true)
        {
            if (!_tempCirclePoints.IsEmpty() && needClear)
            {
                ClearTempCircle();
            }

            int x = 0;
            int y = radius;
            int delta = 3 - 2 * radius;
            DrawCirclePoints(center, x, y, circleColor);
            while (x < y)
            {
                if (delta < 0)
                {
                    delta += 2 * x + 3;
                }
                else
                {
                    delta += 2 * (x - y) + 5;
                    --y;
                }

                ++x;
                DrawCirclePoints(center, x, y, circleColor);
            }
        }

        private void DrawCirclePoints(Vector2Int center, int x, int y, Color circleColor)
        {
            DrawPixel(center.x + x, center.y + y, circleColor, _tempCirclePoints);

            DrawPixel(center.x + y, center.y + x, circleColor, _tempCirclePoints);

            DrawPixel(center.x + y, center.y + (-x), circleColor, _tempCirclePoints);

            DrawPixel(center.x + x, center.y + (-y), circleColor, _tempCirclePoints);

            DrawPixel(center.x + (-x), center.y + (-y), circleColor, _tempCirclePoints);

            DrawPixel(center.x + (-y), center.y + (-x), circleColor, _tempCirclePoints);

            DrawPixel(center.x + (-y), center.y + x, circleColor, _tempCirclePoints);

            DrawPixel(center.x + (-x), center.y + y, circleColor, _tempCirclePoints);
        }
        #endregion

        public void DrawVertexCircle(List<Vector2Int> polygon)
        {
            ClearTempCircle();
            foreach (var vertex in polygon)
            {
                for (int i = 1; i <= _vertexRadius; ++i)
                {
                    DrawCircle(vertex, i, _texture.GetPixel(vertex.x, vertex.y), false);
                }
            }
        }

        private void DrawPixel(int x, int y, Color color, List<OldPoint> tempPoints)
        {
            if (tempPoints != null && tempPoints.Find(point => point.Pos.x == x && point.Pos.y == y) == default(OldPoint))
            {
                tempPoints.Add(new OldPoint()
                {
                    Pos = new Vector2Int(x, y),
                    OldColor = _texture.GetPixel(x, y),
                    CurrentColor = color
                });
            }
            _texture.SetPixel(x, y, color);
        }

        private void ClearTempCircle()
        {
            foreach (var circlePoint in _tempCirclePoints)
            {
                if (_texture.GetPixel(circlePoint.Pos.x, circlePoint.Pos.y) == circlePoint.CurrentColor)
                {
                    _texture.SetPixel(circlePoint.Pos.x, circlePoint.Pos.y, circlePoint.OldColor);
                }
            }
            _texture.Apply();
            _tempCirclePoints.Clear();
        }

        private void ClearTempPoints()
        {
            foreach (var linePoint in _tempLinePoints)
            {
                if (_texture.GetPixel(linePoint.Pos.x, linePoint.Pos.y) == linePoint.CurrentColor)
                {
                    _texture.SetPixel(linePoint.Pos.x, linePoint.Pos.y, linePoint.OldColor);
                }
            }
            _texture.Apply();
            _tempLinePoints.Clear();
        }

        private void Swap(ref int a, ref int b)
        {
            int t = a;
            a = b;
            b = t;
        }
    }
}

