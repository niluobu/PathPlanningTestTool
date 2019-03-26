using Framework.MVP;
using UniRx.Async;
using UnityEngine;

namespace Project.Common
{
    public class SceneHierarchyWidgetsLoader : IWidgetsAssetLoader
    {
        private readonly Transform _root;

        public SceneHierarchyWidgetsLoader(Transform transform)
        {
            _root = transform;
        }

        public UniTask<GameObject> Load<T>()
        {
            Transform transform = _root.Find(typeof(T).Name);
            return UniTask.FromResult(transform?.gameObject);
        }
    }
}
