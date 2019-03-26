using System;
using System.IO;
using UniRx;
using UniRx.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.ResourceManage
{
    internal class AssetBundleManagerResource : BasicSimpleAssetBundleManager, IAssetBundleManager
    {
        private string GetFullPath(string bundlePath, string assetName)
        {
            if (string.IsNullOrEmpty(bundlePath))
            {
                return assetName;
            }

            return Path.Combine(bundlePath, assetName);
        }

        public (T, IDisposable) LoadAsset<T>(string bundlePath, string assetName) where T : Object
        {
            return (Resources.Load<T>(GetFullPath(bundlePath, assetName)), Disposable.Empty);
        }

        public (T, IDisposable) LoadSubAsset<T>(string bundlePath,
            string assetName, string subAssetName)
            where T : Object
        {
            if (string.IsNullOrEmpty(subAssetName))
            {
                return LoadAsset<T>(bundlePath, assetName);
            }

            T[] assets = Resources.LoadAll<T>(GetFullPath(bundlePath, assetName));
            foreach (T asset in assets)
            {
                if (asset.name.Equals(subAssetName))
                {
                    return (asset, Disposable.Empty);
                }
            }

            return (null, Disposable.Empty);
        }

        public async UniTask<(T, IDisposable)> LoadAssetAsync<T>(string bundlePath, string assetName) where T : Object
        {
            T asset = await Resources.LoadAsync<T>(GetFullPath(bundlePath, assetName)) as T;
            return (asset, Disposable.Empty);
        }
    }
}
