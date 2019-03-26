using UniRx.Async;
using UnityEngine;

namespace Framework.MVP
{
    public interface IWidgetsAssetLoader
    {
        UniTask<GameObject> Load<T>();
    }
}
