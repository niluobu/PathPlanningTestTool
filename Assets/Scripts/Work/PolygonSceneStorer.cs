using Framework.Profile;
using System;
using System.Collections.Generic;
using UniRx;
using Zenject;

namespace Project.Work
{
    public interface IPolygonSceneStorer
    {
        IReadOnlyList<PolygonScene> PolygonScenes { get; }
        bool SavePolygonScene(PolygonScene polygonScene);
        IObservable<PolygonScene> AddPolygonScenesAsObservable { get; }
    }

    public class PolygonSceneStorer : IPolygonSceneStorer, IInitializable
    {
        [Inject] private readonly IProfile<PolygonSceneConfig> _profile;

        private List<PolygonScene> _polygonScenes;
        private readonly Subject<PolygonScene> _addScenesSubject = new Subject<PolygonScene>();

        public IObservable<PolygonScene> AddPolygonScenesAsObservable => _addScenesSubject;

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

        public bool SavePolygonScene(PolygonScene polygonScene)
        {
            PolygonScene oldScene = _polygonScenes.Find(x => x.SceneNum == polygonScene.SceneNum);
            if (oldScene != null)
            {
                oldScene = polygonScene;
            }
            else
            {
                _polygonScenes.Add(polygonScene);
                _addScenesSubject.OnNext(polygonScene);
            }
            _profile.Save();
            return true;
        }

    }
}

