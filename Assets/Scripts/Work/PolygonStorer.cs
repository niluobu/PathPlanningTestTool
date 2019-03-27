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
        IReadOnlyList<PolygonScene> PolygonScenes { get; }
        void SavePolygonScene(List<Polygon> polygons);
        IObservable<List<int>> EndSaveSceneAsObservable { get; }
    }

    public class EdgeInfo
    {
        public int PolygonNum;
        public (Vector2Int, Vector2Int) Edge;
    }

    public class PolygonStorer : IPolygonStorer, IInitializable
    {
        [Inject] private readonly IDrawManager _drawManager;
        [Inject] private readonly IProfile<PolygonSceneConfig> _profile;

        private List<PolygonScene> _polygonScenes;
        private readonly Subject<List<int>> _endSaveSceneSubject = new Subject<List<int>>();

        public IObservable<List<int>> EndSaveSceneAsObservable => _endSaveSceneSubject;

        public IReadOnlyList<PolygonScene> PolygonScenes => _polygonScenes;

        public void Initialize()
        {
            _polygonScenes = _profile.Instance.PolygonScenes;
            if (_polygonScenes == null)
            {
                _polygonScenes = new List<PolygonScene>();
                _profile.Instance.PolygonScenes = _polygonScenes;
            }
        }

        public void SavePolygonScene(List<Polygon> polygons)
        {
            GetSceneAllEdge(polygons, out List<EdgeInfo> edgeInfos);
            bool checkSucceed = CheckIntersectingEdge(edgeInfos, out List<int> invalidPolygonNum);
            if (checkSucceed)
            {
                SaveScene(polygons);
            }
            _endSaveSceneSubject.OnNext(invalidPolygonNum);
        }

        private void SaveScene(List<Polygon> polygons)
        {
            int sceneSerialNumber = 2000;
            if (_polygonScenes.Count > 0)
            {
                sceneSerialNumber = _polygonScenes[_polygonScenes.Count - 1].SceneNum + 1;
            }
            _polygonScenes.Add(new PolygonScene()
            {
                SceneNum = sceneSerialNumber,
                Polygons = polygons
            });
            _profile.Save();
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
                        Edge = (polygon.Vertexes[i - 1].Position, polygon.Vertexes[i].Position)
                    });
                }
            }
        }

        #region Check
        private bool CheckIntersectingEdge(List<EdgeInfo> edgeInfos, out List<int> invalidPolygonNum)
        {
            invalidPolygonNum = new List<int>();
            for (int i = 0; i < edgeInfos.Count; ++i)
            {
                for (int j = i + 1; j < edgeInfos.Count; ++j)
                {
                    if (EdgesIsIntersecting(edgeInfos[i], edgeInfos[j]))
                    {
                        invalidPolygonNum.Add(edgeInfos[i].PolygonNum);
                        invalidPolygonNum.Add(edgeInfos[j].PolygonNum);
                    }
                }
            }

            return invalidPolygonNum.Count == 0;
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

