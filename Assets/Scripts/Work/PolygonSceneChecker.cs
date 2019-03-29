using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    public interface IPolygonSceneChecker
    {
        List<int> CheckPolygonScene(PolygonScene polygonScene);
    }

    public class EdgeInfo
    {
        public int PolygonNum;
        public int EdgeNum;
        public (Vector2Int, Vector2Int) Edge;
    }

    public class PolygonSceneChecker : IPolygonSceneChecker
    {
        private const float ApproximateDis = 15f;
        private const float ShortestEdge = 25f;

        public List<int> CheckPolygonScene(PolygonScene polygonScene)
        {
            GetSceneAllEdge(polygonScene.Polygons, out List<EdgeInfo> edgeInfos);
            CheckIntersectingEdge(edgeInfos, out List<int> invalidPolygonNum);
            return invalidPolygonNum;
        }

        private void GetSceneAllEdge(List<Polygon> polygons, out List<EdgeInfo> edgeInfos)
        {
            edgeInfos = new List<EdgeInfo>();
            foreach (var polygon in polygons)
            {
                for (int i = 1; i < polygon.Vertexes.Count; ++i)
                {
                    edgeInfos.Add(new EdgeInfo()
                    {
                        PolygonNum = polygon.PolygonNum,
                        EdgeNum = i - 1,
                        Edge = (polygon.Vertexes[i - 1].Position, polygon.Vertexes[i].Position)
                    });
                }
                edgeInfos.Add(new EdgeInfo()
                {
                    PolygonNum = polygon.PolygonNum,
                    EdgeNum = polygon.Vertexes.Count - 1,
                    Edge = (polygon.Vertexes[0].Position, polygon.Vertexes[polygon.Vertexes.Count - 1].Position)
                });
            }
        }

        #region Check
        private bool CheckIntersectingEdge(List<EdgeInfo> edgeInfos, out List<int> invalidPolygonNum)
        {
            invalidPolygonNum = new List<int>();
            for (int i = 0; i < edgeInfos.Count; ++i)
            {
                if (TooShort(edgeInfos[i]))
                {
                    invalidPolygonNum.Add(edgeInfos[i].PolygonNum);
                    continue;
                }
                for (int j = i + 1; j < edgeInfos.Count; ++j)
                {
                    if (TooShort(edgeInfos[j]))
                    {
                        invalidPolygonNum.Add(edgeInfos[j].PolygonNum);
                        continue;
                    }
                    if (EdgesIsIntersecting(edgeInfos[i], edgeInfos[j]))
                    {
                        invalidPolygonNum.Add(edgeInfos[i].PolygonNum);
                        invalidPolygonNum.Add(edgeInfos[j].PolygonNum);
                    }
                }
            }

            return invalidPolygonNum.Count == 0;
        }

        private bool TooShort(EdgeInfo e)
        {
            return Vector2Int.Distance(e.Edge.Item1, e.Edge.Item2) < ShortestEdge;
        }

        private bool EdgesIsIntersecting(EdgeInfo a, EdgeInfo b)
        {
            if (a.PolygonNum == b.PolygonNum && IsConnecting(a, b))
            {
                return false;
            }

            if (FastMutexes(a, b))
            {
                return false;
            }

            Vector2Int b1a1 = b.Edge.Item1 - a.Edge.Item1;
            Vector2Int b1a2 = b.Edge.Item1 - a.Edge.Item2;
            Vector2Int b2a1 = b.Edge.Item2 - a.Edge.Item1;
            Vector2Int b2a2 = b.Edge.Item2 - a.Edge.Item2;
            float m1 = CrossProduct(b1a1, b1a2) * CrossProduct(b2a1, b2a2);
            Vector2Int a1b1 = a.Edge.Item1 - b.Edge.Item1;
            Vector2Int a1b2 = a.Edge.Item1 - b.Edge.Item2;
            Vector2Int a2b1 = a.Edge.Item2 - b.Edge.Item1;
            Vector2Int a2b2 = a.Edge.Item2 - b.Edge.Item2;
            float m2 = CrossProduct(a1b1, a1b2) * CrossProduct(a2b1, a2b2);
            if (m1 <= 0 && m2 <= 0)
            {
                return true;
            }

            return TooClose(a, b);
        }

        private bool TooClose(EdgeInfo a, EdgeInfo b)
        {
            float minDis = Mathf.Min(PointToEdgeShortestDis(a.Edge.Item1, b), PointToEdgeShortestDis(a.Edge.Item2, b),
                PointToEdgeShortestDis(b.Edge.Item1, a), PointToEdgeShortestDis(b.Edge.Item2, a));
            //Debug.Log($"polygon {a.PolygonNum} edge {a.EdgeNum} with polygon {b.PolygonNum} edge {b.EdgeNum} misDis : {minDis}");
            return minDis < ApproximateDis;
        }

        private float PointToEdgeShortestDis(Vector2Int pos, EdgeInfo e)
        {
            float eSquareDis = SquareDis(e.Edge.Item1, e.Edge.Item2);
            Vector2 e1_p = pos - e.Edge.Item1;
            Vector2 e1_e2 = e.Edge.Item2 - e.Edge.Item1;
            float r = DotProduct(e1_p, e1_e2) / eSquareDis;
            if (r <= 0)
            {
                return Vector2Int.Distance(e.Edge.Item1, pos);
            }

            if (r >= 1)
            {
                return Vector2Int.Distance(e.Edge.Item2, pos);
            }

            Vector2 e1_e3 = new Vector2(e1_e2.x * r, e1_e2.y * r);
            Vector2 e3_p = e1_p - e1_e3;
            return e3_p.magnitude;
        }

        private float SquareDis(Vector2 a, Vector2 b)
        {
            return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        }

        private bool FastMutexes(EdgeInfo a, EdgeInfo b)
        {
            if (
                (Mathf.Min(a.Edge.Item1.x, a.Edge.Item2.x) > Mathf.Max(b.Edge.Item1.x, b.Edge.Item2.x) ||
                Mathf.Max(a.Edge.Item1.x, a.Edge.Item2.x) < Mathf.Min(b.Edge.Item1.x, b.Edge.Item2.x)) ||
                (Mathf.Min(a.Edge.Item1.y, a.Edge.Item2.y) > Mathf.Max(b.Edge.Item1.y, b.Edge.Item2.y) ||
                 Mathf.Max(a.Edge.Item1.y, a.Edge.Item2.y) < Mathf.Min(b.Edge.Item1.y, b.Edge.Item2.y))
                )
            {
                return true;
            }

            return false;
        }

        private bool IsConnecting(EdgeInfo a, EdgeInfo b)
        {
            if (a.Edge.Item1 == b.Edge.Item1 || a.Edge.Item1 == b.Edge.Item2 ||
                a.Edge.Item2 == b.Edge.Item1 || a.Edge.Item2 == b.Edge.Item2)
            {
                return true;
            }

            return false;
        }

        private float DotProduct(Vector2 a, Vector2 b) //向量数量积
        {
            return a.x * b.x + a.y * b.y;
        }

        private float CrossProduct(Vector2 a, Vector2 b) //向量叉乘
        {
            return a.x * b.y - b.x * a.y;
        }
        #endregion
    }
}

