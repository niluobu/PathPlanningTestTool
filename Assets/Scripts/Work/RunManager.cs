using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Project.Work
{
    public interface IRunManager
    {
        void DrawScene(PolygonScene scene);
        void ClearDrawPanel();
        void StartTest(Vector2Int startPoint, Vector2Int endPoint);
        void GetTestParameter(out int vertexNum, out int edgeNum, out int polygonEdgeNum);
        IObservable<string> TestStageHintAsObservable { get; }
        IObservable<Unit> TestEndAsObservable { get; }
    }

    public class VertexInfo
    {
        public int Num;
        public Vector2Int Pos;
        public int PolygonNum;
        public (int, int) ConnectEdgesNum;
    }

    public class RunManager : IRunManager
    {
        [Inject] private readonly IDrawPanelPreference _drawPanelPreference;
        [Inject] private readonly IGlUtil _glUtil;
        [Inject] private readonly ProjectSetting _projectSetting;
        [Inject] private readonly IVisibleGraphUtil _visibleGraphUtil;
        [Inject] private readonly IDijkstraAlgorithm _dijkstraAlgorithm;

        private readonly Subject<string> _hintSubject = new Subject<string>();
        private readonly Subject<Unit> _endSubject = new Subject<Unit>();

        private PolygonScene _scene;
        private bool[,] _sceneVg;
        private bool[,] _appendVg;
        private List<VertexInfo> _vertexes;
        private List<EdgeInfo> _edges;
        private VertexInfo _startPoint;
        private VertexInfo _endPoint;
        private float[,] _adjacentM;
        private bool _isNewScene;
        private float _CVGTime = 0;
        private float _SPTime = 0;

        public IObservable<string> TestStageHintAsObservable => _hintSubject;

        public IObservable<Unit> TestEndAsObservable => _endSubject;

        public void GetTestParameter(out int vertexNum, out int edgeNum, out int polygonEdgeNum)
        {
            vertexNum = _vertexes.Count + 2;
            edgeNum = GetVisibleGraphEdgeNum();
            polygonEdgeNum = _edges.Count;
        }

        private int GetVisibleGraphEdgeNum()
        {
            int count = 0;
            int n = _adjacentM.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (_adjacentM[i, j] != 0 && _adjacentM[i, j] != float.MaxValue)
                    {
                        ++count;
                    }
                }
            }
            return count;
        }

        public void ClearDrawPanel()
        {
            _drawPanelPreference.SetTextureBgColor();
        }

        public void StartTest(Vector2Int startPoint, Vector2Int endPoint)
        {
            InitSEPoints(startPoint, endPoint);
            if (_visibleGraphUtil.Visible(_startPoint, _endPoint, _edges, _scene.Polygons))
            {
                _glUtil.DrawLine(_startPoint.Pos, _endPoint.Pos, _projectSetting.ResultPathLineColor, _projectSetting.ResultPathLineWide);
                EndPathPlan();
                return;
            }

            CreateVisibleGraph();
            ShortestPathPlanning();
            EndPathPlan();
        }

        private void EndPathPlan()
        {
            _drawPanelPreference.Apply();
            _endSubject.OnNext(Unit.Default);
        }

        #region ShortestPathPlanning
        private void ShortestPathPlanning()
        {
            _SPTime = Time.time;
            _hintSubject.OnNext("正在进行路径规划...");
            if (_isNewScene)
            {
                CreateSceneAdjacentMatrix();
            }
            else
            {
                UpdateSEAdjacentInfo();
            }

            List<int> path = _dijkstraAlgorithm.PathPlanning(_adjacentM, _vertexes.Count, _vertexes.Count + 1);
            DrawShortestPath(path);
            _SPTime = Time.time - _SPTime;
            _hintSubject.OnNext("已规划出最短路径！");
        }

        private void DrawShortestPath(List<int> path)
        {
            if (path == null)
            {
                Debug.LogError("path planning error");
                return;
            }
            _glUtil.DrawLine(_startPoint.Pos, _vertexes[path[0]].Pos,
                _projectSetting.ResultPathLineColor, _projectSetting.ResultPathLineWide);
            for (int i = 1; i < path.Count; ++i)
            {
                _glUtil.DrawLine(_vertexes[path[i - 1]].Pos, _vertexes[path[i]].Pos,
                    _projectSetting.ResultPathLineColor, _projectSetting.ResultPathLineWide);
            }
            _glUtil.DrawLine(_vertexes[path[path.Count - 1]].Pos, _endPoint.Pos,
                _projectSetting.ResultPathLineColor, _projectSetting.ResultPathLineWide);
        }

        private void CreateSceneAdjacentMatrix()
        {
            _adjacentM = new float[_vertexes.Count + 2, _vertexes.Count + 2];
            for (int i = 0; i < _vertexes.Count; ++i)
            {
                for (int j = 0; j < _vertexes.Count; ++j)
                {
                    if (i == j)
                    {
                        _adjacentM[i, j] = 0f;
                        continue;
                    }
                    if (_sceneVg[i, j])
                    {
                        _adjacentM[i, j] = Vector2Int.Distance(_vertexes[i].Pos, _vertexes[j].Pos);
                        continue;
                    }

                    _adjacentM[i, j] = float.MaxValue;
                }
            }

            UpdateSEAdjacentInfo();
        }

        private void UpdateSEAdjacentInfo()
        {
            int n = _vertexes.Count;
            _adjacentM[n, n] = 0f;
            _adjacentM[n + 1, n + 1] = 0f;
            _adjacentM[n, n + 1] = float.MaxValue;
            _adjacentM[n + 1, n] = float.MaxValue;

            for (int j = 0; j < _vertexes.Count; ++j)
            {
                _adjacentM[n, j] = _appendVg[0, j] == true ?
                    Vector2Int.Distance(_startPoint.Pos, _vertexes[j].Pos) : float.MaxValue;
                _adjacentM[j, n] = _adjacentM[n, j];

                _adjacentM[n + 1, j] = _appendVg[1, j] == true ?
                    Vector2Int.Distance(_endPoint.Pos, _vertexes[j].Pos) : float.MaxValue;
                _adjacentM[j, n + 1] = _adjacentM[n + 1, j];
            }
        }

        #endregion

        #region CreateVisibleGraph
        private void CreateVisibleGraph()
        {
            _CVGTime = Time.time;
            _hintSubject.OnNext("正在构造可见性图...");
            if (_isNewScene)
            {
                ProcessSceneVisibleGraph();
            }
            ProcessAppendVisibleGraph();

            _visibleGraphUtil.DrawSceneVisibleGraph(_sceneVg, _vertexes);
            _visibleGraphUtil.DrawAppendVisibleGraph(_appendVg, _vertexes, _startPoint, _endPoint);

            _CVGTime = Time.time - _CVGTime;
            _hintSubject.OnNext("可见性图构造结束！");
        }

        private void ProcessSceneVisibleGraph()
        {
            _sceneVg = new bool[_vertexes.Count, _vertexes.Count];
            for (int i = 0; i < _vertexes.Count; ++i)
            {
                for (int j = i + 1; j < _vertexes.Count; ++j)
                {
                    if (_visibleGraphUtil.Visible(_vertexes[i], _vertexes[j], _edges, _scene.Polygons))
                    {
                        _sceneVg[i, j] = true;
                    }
                }
            }
        }

        private void ProcessAppendVisibleGraph()
        {
            _appendVg = new bool[2, _vertexes.Count];

            for (int j = 0; j < _vertexes.Count; j++)
            {
                if (_visibleGraphUtil.Visible(_startPoint, _vertexes[j], _edges, _scene.Polygons))
                {
                    _appendVg[0, j] = true;
                }

                if (_visibleGraphUtil.Visible(_endPoint, _vertexes[j], _edges, _scene.Polygons))
                {
                    _appendVg[1, j] = true;
                }
            }
        }
        #endregion

        #region DrawScene
        public void DrawScene(PolygonScene scene)
        {
            _isNewScene = false;
            if (_scene == null || _scene.SceneNum != scene.SceneNum)
            {
                _scene = scene;
                InitSceneVertexesAndEdges();
                _isNewScene = true;
            }
            DrawPolygon();
            _drawPanelPreference.Apply();
        }

        private void DrawPolygon()
        {
            _scene.Polygons.Sort((a, b) => a.GetMinX() - b.GetMinX());
            List<int> insidePolygonNums = new List<int>();
            foreach (Polygon polygon in _scene.Polygons)
            {
                if (!_glUtil.FillPolygon(polygon, _projectSetting.FillColor, _drawPanelPreference.Texture,
                    _drawPanelPreference.ConvertToDrawPanelPos))
                {
                    insidePolygonNums.Add(polygon.PolygonNum);
                }
            }

            foreach (var insidePolygonNum in insidePolygonNums)
            {
                Polygon insidePolygon = _scene.Polygons.Find(x => x.PolygonNum == insidePolygonNum);
                if (insidePolygon != null)
                {
                    _scene.Polygons.Remove(insidePolygon);
                }
            }
        }
        #endregion

        #region Init
        private void InitSEPoints(Vector2Int startPoint, Vector2Int endPoint)
        {
            _startPoint = new VertexInfo()
            {
                Num = _vertexes.Count,
                Pos = startPoint,
                PolygonNum = int.MinValue,
                ConnectEdgesNum = (int.MinValue, int.MinValue)
            };
            _endPoint = new VertexInfo()
            {
                Num = _vertexes.Count + 1,
                Pos = endPoint,
                PolygonNum = int.MaxValue,
                ConnectEdgesNum = (int.MaxValue, int.MaxValue)
            };
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
        #endregion

    }
}
