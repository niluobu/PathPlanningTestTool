using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Project.Work
{
    public interface IVisibleGraphUtil
    {
        void DrawSceneVisibleGraph(bool[,] sceneVg, List<VertexInfo> vertexes);

        void DrawAppendVisibleGraph(bool[,] appendVg, List<VertexInfo> vertexes, VertexInfo startPoint,
            VertexInfo endPoint);

        bool Visible(VertexInfo a, VertexInfo b, List<EdgeInfo> edges, List<Polygon> polygons);
    }

    internal class VisibleGraphUtil : IVisibleGraphUtil
    {
        [Inject] private readonly IPolygonSceneChecker _polygonSceneChecker;
        [Inject] private readonly ProjectSetting _projectSetting;
        [Inject] private readonly IGlUtil _glUtil;
        [Inject] private readonly IDrawPanelPreference _drawPanelPreference;

        public void DrawSceneVisibleGraph(bool[,] sceneVg, List<VertexInfo> vertexes)
        {
            for (int i = 0; i < vertexes.Count; ++i)
            {
                for (int j = 0; j < vertexes.Count; ++j)
                {
                    if (sceneVg[i, j])
                    {
                        _glUtil.DrawLine(vertexes[i].Pos, vertexes[j].Pos, _projectSetting.VisibleGLineColor, _projectSetting.VisibleGLineWide);
                    }
                }
            }
        }

        public void DrawAppendVisibleGraph(bool[,] appendVg, List<VertexInfo> vertexes, VertexInfo startPoint, VertexInfo endPoint)
        {
            for (int i = 0; i < vertexes.Count; ++i)
            {
                if (appendVg[0, i])
                {
                    _glUtil.DrawLine(vertexes[i].Pos, startPoint.Pos, _projectSetting.VisibleGLineColor, _projectSetting.VisibleGLineWide);
                }

                if (appendVg[1, i])
                {
                    _glUtil.DrawLine(vertexes[i].Pos, endPoint.Pos, _projectSetting.VisibleGLineColor, _projectSetting.VisibleGLineWide);
                }
            }
        }

        public bool Visible(VertexInfo a, VertexInfo b, List<EdgeInfo> edges, List<Polygon> polygons)
        {
            if (a.ConnectEdgesNum.Item1 == b.ConnectEdgesNum.Item2 ||
                a.ConnectEdgesNum.Item2 == b.ConnectEdgesNum.Item1)
            {
                return true;
            }

            foreach (var edge in edges)
            {
                if (IsOverlap((a.Pos, b.Pos), edge.Edge))
                {
                    return false;
                }

                if (_polygonSceneChecker.IsConnecting((a.Pos, b.Pos), edge.Edge))
                {
                    continue;
                }

                if (_polygonSceneChecker.IsIntersect((a.Pos, b.Pos), edge.Edge))
                {
                    return false;
                }
            }

            if (a.PolygonNum == b.PolygonNum)
            {
                return !IsAcrossPolygon((a.Pos, b.Pos), polygons.Find(x => x.PolygonNum == a.PolygonNum));
            }

            return true;
        }

        private bool IsAcrossPolygon((Vector2Int, Vector2Int) p, Polygon polygon)
        {
            int n = polygon.Vertexes.Count;
            for (int i = 0; i < n; ++i)
            {
                (Vector2Int, Vector2Int) e = (polygon.Vertexes[i].Position, polygon.Vertexes[(i + 1) % n].Position);
                if (_polygonSceneChecker.IsConnecting(p, e))
                {
                    continue;
                }

                if (_polygonSceneChecker.IsIntersect(p, e))
                {
                    return true;
                }
            }

            Vector2Int m = _drawPanelPreference.ConvertToDrawPanelPos(GetMiddle(p.Item1, p.Item2));
            return _drawPanelPreference.Texture.GetPixel(m.x, m.y) != _projectSetting.TextureBgColor;
        }

        private bool IsOverlap((Vector2Int, Vector2Int) a, (Vector2Int, Vector2Int) b)
        {
            if (_polygonSceneChecker.FastMutexes(a, b))
            {
                return false;
            }

            float angle = GetClockwiseAngle(a.Item2 - a.Item1, b.Item2 - b.Item1);
            if (angle != 0 && angle != 180f)
            {//不平行
                return false;
            }

            angle = GetClockwiseAngle(a.Item1 - a.Item2, a.Item1 - b.Item1);
            if (angle != 0 && angle != 180f)
            {//不共线
                return false;
            }

            return true;
        }

        private Vector2Int GetMiddle(Vector2Int a, Vector2Int b)
        {
            int x = (a.x + b.x) / 2;
            int y = (a.y + b.y) / 2;
            return new Vector2Int(x, y);
        }

        #region no use
        private int PartitionEdgeByRay(Vector2Int s, Vector2Int rayDir, EdgeInfo edge)
        {
            Vector2Int se1 = edge.Edge.Item1 - s;
            Vector2Int se2 = edge.Edge.Item2 - s;
            float angle1 = GetClockwiseAngle(rayDir, se1);
            float angle2 = GetClockwiseAngle(rayDir, se2);
            float angle3 = GetClockwiseAngle(se1, se2);
            if (angle3 <= 180f && angle1 > 180f && angle2 < 180)
            {//相交，边的两端点在射线两边，且端点和s所成的角小于180
                return 0;
            }

            if (angle1 > 180f && angle2 > 180f)
            {//两个端点都在在射线逆时针一侧
                return -1;
            }

            return 1;
        }

        private void SortVertexesWithAngle(Vector2Int pos, List<VertexInfo> vertexes)
        {
            vertexes.Sort((a, b) =>
            {
                float angle1 = GetClockwiseAngle(pos - a.Pos, pos - Vector2Int.right);
                float angle2 = GetClockwiseAngle(pos - b.Pos, pos - Vector2Int.right);
                if (angle1 != angle2)
                {
                    return angle1 - angle2 < 0 ? -1 : 1;
                }

                return Vector2Int.Distance(pos, a.Pos) - Vector2Int.Distance(pos, b.Pos) < 0 ?
                    -1 : 1;
            });
        }

        private float GetClockwiseAngle(Vector2Int from, Vector2Int to)
        {
            float angle = Vector3.SignedAngle(new Vector3(from.x, from.y, 0), new Vector3(to.x, to.y, 0), Vector3.back);
            if (angle < 0)
            {
                angle = 360f + angle;
            }
            return angle;
        }
        #endregion
    }
}
