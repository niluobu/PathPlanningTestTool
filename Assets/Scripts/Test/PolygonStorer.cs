using Framework.Profile;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Project.Work
{
    public interface IPolygonStorer
    {
        void StorePolygon(List<Vector2Int> vertexex);
        void SavePolygonScene();
        IObservable<bool> EndSaveSceneAsObservable { get; }
    }

    public class EdgeInfo
    {
        public int PolygonNum;
        public (Vector2Int, Vector2Int) Edge;
    }

    public class PolygonStorer : IPolygonStorer, IInitializable
    {
        [Inject] private readonly IDrawManager _drawManager;
        [Inject] private readonly IProfile<SceneDataList> _sceneDataProfile;

        private readonly Dictionary<int, PolygonData> _polygons = new Dictionary<int, PolygonData>();
        private int _polygonSerialNumber = 1000;
        private readonly Subject<bool> _endSaveSceneSubject = new Subject<bool>();
        private SceneDataList _sceneDataList;

        public IObservable<bool> EndSaveSceneAsObservable => _endSaveSceneSubject;

        public void Initialize()
        {
            _sceneDataList = _sceneDataProfile.Instance;
            if (_sceneDataList.PolygonSceneDatas == null)
            {
                _sceneDataList.PolygonSceneDatas = new List<PolygonSceneData>();
            }
        }

        public void StorePolygon(List<Vector2Int> vertexex)
        {
            PolygonData polygon = new PolygonData()
            {
                PolygonNum = _polygonSerialNumber,
                Vertexes = vertexex
            };

            _polygons.Add(_polygonSerialNumber++, polygon);
        }

        public void SavePolygonScene()
        {
            GetSceneAllEdge(out List<EdgeInfo> edgeInfos);
            bool checkSucceed = CheckIntersectingEdge(edgeInfos);
            if (checkSucceed)
            {
                SaveScene();
            }
            _endSaveSceneSubject.OnNext(checkSucceed);
        }

        private void SaveScene()
        {
            int sceneSerialNumber = 2000;
            if (_sceneDataList.PolygonSceneDatas.Count > 0)
            {
                sceneSerialNumber = _sceneDataList.PolygonSceneDatas[_sceneDataList.PolygonSceneDatas.Count - 1].SceneNum + 1;
            }
            PolygonSceneData scene = new PolygonSceneData()
            {
                SceneNum = sceneSerialNumber,
                PolygonDatas = new List<PolygonData>(_polygons.Values)
            };
            _sceneDataList.PolygonSceneDatas.Add(scene);
            _sceneDataProfile.Save();
        }

        private void GetSceneAllEdge(out List<EdgeInfo> edgeInfos)
        {
            edgeInfos = new List<EdgeInfo>();
            foreach (var polygon in _polygons)
            {
                for (int i = 1; i < polygon.Value.Vertexes.Count; ++i)
                {
                    edgeInfos.Add(new EdgeInfo()
                    {
                        PolygonNum = polygon.Key,
                        Edge = (polygon.Value.Vertexes[i - 1], polygon.Value.Vertexes[i])
                    });
                }
            }
        }

        #region Check
        private bool CheckIntersectingEdge(List<EdgeInfo> edgeInfos)
        {
            List<int> intersectingPolygonNum = new List<int>();
            for (int i = 0; i < edgeInfos.Count; ++i)
            {
                for (int j = i + 1; j < edgeInfos.Count; ++j)
                {
                    if (EdgesIsIntersecting(edgeInfos[i], edgeInfos[j]))
                    {
                        intersectingPolygonNum.Add(edgeInfos[i].PolygonNum);
                        intersectingPolygonNum.Add(edgeInfos[j].PolygonNum);
                    }
                }
            }

            bool checkSucceed = true;
            foreach (int num in intersectingPolygonNum)
            {
                if (_polygons.ContainsKey(num))
                {
                    _polygons.Remove(num);
                    checkSucceed = false;
                }
            }

            return checkSucceed;
        }

        private bool EdgesIsIntersecting(EdgeInfo a, EdgeInfo b)
        {
            Vector2Int s1 = b.Edge.Item1 - a.Edge.Item1;
            Vector2Int s2 = b.Edge.Item1 - a.Edge.Item2;
            Vector2Int s3 = b.Edge.Item1 - b.Edge.Item2;
            int d = CrossProduct(s1, s3) * CrossProduct(s2, s3);
            if (d > 0)
            {
                return false;
            }
            if (d < 0)
            {
                return true;
            }
            return a.PolygonNum != b.PolygonNum;
        }

        private int CrossProduct(Vector2Int a, Vector2Int b)
        {
            return a.x * b.y - b.x * a.y;
        }
        #endregion
    }
}

