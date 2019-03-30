using System.Collections.Generic;
using UnityEngine;

namespace Project.Work
{
    public interface IVisibleGraphUtil
    {

    }

    internal class VisibleGraphUtil : IVisibleGraphUtil
    {
        public class VertexInfo
        {
            public int Num;
            public Vector2Int Pos;
            public int PolygonNum;
            public (int, int) ConnectEdgesNum;
        }

        private bool[,] _sceneVg;
        private bool[,] _apendVg;
        private List<VertexInfo> _vertexes;
        private List<EdgeInfo> _edges;
        private Vector2Int _startPoint;
        private Vector2Int _endPoint;
        private PolygonScene _scene;

        public void CreateVisibleGraph(PolygonScene scene, Vector2Int startPoint, Vector2Int endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
            if (_scene != null && _scene.SceneNum == scene.SceneNum)
            {
                ProcessAppendVisibleGraph();
            }
            else
            {
                _scene = scene;
                InitSceneVertexesAndEdges();
                ProcessSceneVisibleGraph();
                ProcessAppendVisibleGraph();
            }
        }

        private void InitSceneVertexesAndEdges()
        {
            _vertexes = new List<VertexInfo>();
            _edges = new List<EdgeInfo>();
            int vertexNum = 0;
            int edgeNum = 0;
            foreach (var polygon in _scene.Polygons)
            {
                int n = polygon.Vertexes.Count;
                for (int i = 0; i < n; ++i)
                {
                    _edges.Add(new EdgeInfo()
                    {
                        Edge = (polygon.Vertexes[i].Position, polygon.Vertexes[(i + 1) % n].Position),
                        EdgeNum = edgeNum,
                        PolygonNum = polygon.PolygonNum
                    });
                    _vertexes.Add(new VertexInfo()
                    {
                        Pos = polygon.Vertexes[i].Position,
                        PolygonNum = polygon.PolygonNum,
                        Num = vertexNum,
                        ConnectEdgesNum = (i - 1 < 0 ? (vertexNum + n - 1) : vertexNum - 1, vertexNum)
                    });
                    ++vertexNum;
                    ++edgeNum;
                }
            }
        }

        private void ProcessSceneVisibleGraph()
        {
            _sceneVg = new bool[_vertexes.Count, _vertexes.Count];
            for (int i = 0; i < _vertexes.Count; ++i)
            {
                VisibilityVertexes(_vertexes[i].Pos, out List<int> visibleSceneVertexes);
                foreach (var j in visibleSceneVertexes)
                {
                    _sceneVg[i, j] = true;
                }
            }
        }

        private void ProcessAppendVisibleGraph()
        {

        }

        private void VisibilityVertexes(Vector2Int pos, out List<int> visibleSceneVertexes)
        {
            visibleSceneVertexes = new List<int>();
            SortVertexesWithAngle(pos);
            foreach (var vertex in _vertexes)
            {

            }
        }

        private int PartitionEdgeByRay(Vector2Int s, Vector2Int rayDir, EdgeInfo edge)
        {
            Vector2Int se1 = edge.Edge.Item1 - s;
            Vector2Int se2 = edge.Edge.Item2 - s;
            float angle1 = GetClockwiseAngle(rayDir, se1);
            float angle2 = GetClockwiseAngle(rayDir, se2);
            if (angle1 > 180f && angle2 > 180f)
            {
                return -1;
            }

            if (angle1 < 180f && angle2 < 180f)
            {
                return 1;
            }

            if ((angle1 < 90f && angle2 > 180f) || (angle2 < 90f && angle1 > 180f))
            {
                return 0;
            }

            return 1;
        }

        private void SortVertexesWithAngle(Vector2Int pos)
        {
            _vertexes.Sort((a, b) =>
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
                angle = 180f - angle;
            }
            return angle;
        }
    }
}
