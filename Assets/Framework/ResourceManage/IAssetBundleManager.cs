using System;
using UniRx.Async;

namespace Framework.ResourceManage
{
    public interface IAssetBundleManager
    {
        (T, IDisposable) LoadAsset<T>(string bundlePath, string assetName)
            where T : UnityEngine.Object;
        (T, IDisposable) LoadSubAsset<T>(string bundlePath, string assetName, string subAssetName)
            where T : UnityEngine.Object;
        UniTask<(T, IDisposable)> LoadAssetAsync<T>(string bundlePath, string assetName)
            where T : UnityEngine.Object;
        IDisposable LoadBundle(string path);
        UniTask<IDisposable> LoadBundleAsync(string path);
    }
}
