using ModestTree;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    public interface IGlUtil
    {
        void DrawLine(Vector2Int startPoint, Vector2Int endPoint, Color lineColor, Texture2D texture);
        bool FillPolygon(Polygon polygon, Color fillColor, Texture2D texture, Func<Vector2Int, Vector2Int> convertPosFunc);
    }

    public class GlUtil : IGlUtil
    {
        #region DrawLine
        public void DrawLine(Vector2Int startPoint, Vector2Int endPoint, Color lineColor, Texture2D texture)
        {
            DrawLine(startPoint.x, startPoint.y, endPoint.x, endPoint.y, lineColor, texture);
        }

        private void DrawLine(int x0, int y0, int x1, int y1, Color lineColor, Texture2D texture)
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
                    DrawPixel(y, x, lineColor, texture);
                }
                else
                {
                    DrawPixel(x, y, lineColor, texture);
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

        #region Area filling
        public class Edge
        {
            public float BottomX;
            public float SlopeInverse;
            public int MaxY;
            public int MinY;
        }

        public bool FillPolygon(Polygon polygon, Color fillColor, Texture2D texture, Func<Vector2Int, Vector2Int> convertPosFunc)
        {
            if (InsideCheck(polygon, fillColor, texture))
            {
                return false;
            }
            GetEdgeTable(out Dictionary<int, List<Edge>> edgeTable, polygon.Vertexes, convertPosFunc);
            GetEdgesRangeOfY(edgeTable, out int minY, out int maxY);
            List<Edge> aet = new List<Edge>();
            for (int y = minY; y <= maxY; ++y)
            {
                DeleteOutEdge(aet, y);
                UpdateIntersectionPoint(aet);
                if (edgeTable.ContainsKey(y))
                {
                    InsertNewEdge(aet, edgeTable[y]);
                }

                Fill(aet, y, fillColor, texture);
            }

            return true;
        }

        private bool InsideCheck(Polygon polygon, Color fillColor, Texture2D texture)
        {
            Color pointColor = texture.GetPixel(polygon.Vertexes[0].Position.x, polygon.Vertexes[0].Position.y);
            //Debug.Log($"fillColor: {fillColor} pointColor: {pointColor} equal: {fillColor == pointColor}");
            return fillColor == pointColor;
        }

        private void Fill(List<Edge> aet, int y, Color fillColor, Texture2D texture)
        {
            if (aet.Count % 2 != 0)
            {
                Debug.LogError("Active edge table count is not even number");
                return;
            }

            for (int i = 0; i < aet.Count - 1; i += 2)
            {
                FillRange(aet[i].BottomX, aet[i + 1].BottomX, y, fillColor, texture);
            }
        }

        private void FillRange(float left, float right, int y, Color fillColor, Texture2D texture)
        {
            int max = Mathf.RoundToInt(right);
            for (int x = Mathf.RoundToInt(left); x <= max; ++x)
            {
                DrawPixel(x, y, fillColor, texture);
            }
        }

        private void UpdateIntersectionPoint(List<Edge> aet)
        {
            if (aet.IsEmpty())
            {
                return;
            }

            foreach (var edge in aet)
            {
                edge.BottomX += edge.SlopeInverse;
            }
        }

        private void InsertNewEdge(List<Edge> aet, List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                aet.Add(edge);
            }
            aet.Sort((a, b) =>
            {
                if (a.BottomX != b.BottomX)
                {
                    return a.BottomX < b.BottomX ? -1 : 1;
                }

                return a.SlopeInverse < b.SlopeInverse ? -1 : 1;
            });
        }

        private void DeleteOutEdge(List<Edge> aet, int y)
        {
            if (aet.IsEmpty())
            {
                return;
            }

            for (int i = aet.Count - 1; i >= 0; --i)
            {
                if (aet[i].MaxY <= y)
                {
                    aet.RemoveAt(i);
                }
            }
        }

        private void GetEdgesRangeOfY(Dictionary<int, List<Edge>> edgeTable, out int minY, out int maxY)
        {
            minY = int.MaxValue;
            maxY = int.MinValue;
            foreach (var edges in edgeTable.Values)
            {
                foreach (var edge in edges)
                {
                    minY = Mathf.Min(minY, edge.MinY);
                    maxY = Mathf.Max(maxY, edge.MaxY);
                }
            }
        }

        private void GetEdgeTable(out Dictionary<int, List<Edge>> edgeTable, List<Vertex> vertexes, Func<Vector2Int, Vector2Int> convertPosFunc)
        {
            edgeTable = new Dictionary<int, List<Edge>>();
            for (int i = 0; i < vertexes.Count; ++i)
            {
                Vector2Int x1 = convertPosFunc(vertexes[i].Position);
                Vector2Int x2 = convertPosFunc(vertexes[(i + 1) % vertexes.Count].Position);
                if (x1.y == x2.y)
                {
                    continue;
                }
                Edge edge = new Edge()
                {
                    BottomX = x1.y < x2.y ? x1.x : x2.x,
                    MaxY = Mathf.Max(x1.y, x2.y),
                    MinY = Mathf.Min(x1.y, x2.y),
                    SlopeInverse = (float)(x2.x - x1.x) / (x2.y - x1.y)
                };
                if (edgeTable.ContainsKey(edge.MinY))
                {
                    edgeTable[edge.MinY].Add(edge);
                    continue;
                }
                edgeTable.Add(edge.MinY, new List<Edge>() { edge });
            }
        }

        #endregion

        //#region DrawCircle
        //public void DrawCircle(Vector2Int center, int radius, Color circleColor)
        //{
        //    int x = 0;
        //    int y = radius;
        //    int delta = 3 - 2 * radius;
        //    DrawCirclePoints(center, x, y, circleColor);
        //    while (x < y)
        //    {
        //        if (delta < 0)
        //        {
        //            delta += 2 * x + 3;
        //        }
        //        else
        //        {
        //            delta += 2 * (x - y) + 5;
        //            --y;
        //        }

        //        ++x;
        //        DrawCirclePoints(center, x, y, circleColor);
        //    }
        //}

        //private void DrawCirclePoints(Vector2Int center, int x, int y, Color circleColor)
        //{
        //    DrawPixel(center.x + x, center.y + y, circleColor);

        //    DrawPixel(center.x + y, center.y + x, circleColor);

        //    DrawPixel(center.x + y, center.y + (-x), circleColor);

        //    DrawPixel(center.x + x, center.y + (-y), circleColor);

        //    DrawPixel(center.x + (-x), center.y + (-y), circleColor);

        //    DrawPixel(center.x + (-y), center.y + (-x), circleColor);

        //    DrawPixel(center.x + (-y), center.y + x, circleColor);

        //    DrawPixel(center.x + (-x), center.y + y, circleColor);
        //}
        //#endregion

        private void DrawPixel(int x, int y, Color color, Texture2D texture)
        {
            texture.SetPixel(x, y, color);
        }

        private void Swap(ref int a, ref int b)
        {
            int t = a;
            a = b;
            b = t;
        }
    }
}

