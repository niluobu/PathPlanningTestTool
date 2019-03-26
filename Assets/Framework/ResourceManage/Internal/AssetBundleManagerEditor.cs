#if UNITY_EDITOR
using System;
using UniRx;
using UniRx.Async;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Framework.ResourceManage
{
    internal class AssetBundleManagerEditor : BasicSimpleAssetBundleManager, IAssetBundleManager
    {
        private static string GetAssetPath(string path, string asset)
        {
            string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(path, asset);
            if (assetPaths.Length == 0)
            {
                return null;
            }
            return assetPaths[0];
        }

        public (T, IDisposable) LoadAsset<T>(string bundlePath, string assetName) where T : Object
        {
            string filePath = GetAssetPath(bundlePath, assetName);
            if (string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException($"[{bundlePath}] [{assetName}] not found");
            }

            T asset = AssetDatabase.LoadAssetAtPath<T>(filePath);
            if (asset == null)
            {
                throw new InvalidOperationException($"[{filePath}] load failed");
            }

            return (asset, Disposable.Empty);
        }

        public (T, IDisposable) LoadSubAsset<T>(string bundlePath,
            string assetName, string subAssetName)
            where T : Object
        {
            if (string.IsNullOrEmpty(subAssetName))
            {
                return LoadAsset<T>(bundlePath, assetName);
            }

            string filePath = GetAssetPath(bundlePath, assetName);
            if (string.IsNullOrEmpty(filePath))
            {
                throw new InvalidOperationException($"[{bundlePath}] [{assetName}] not found");
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(filePath);
            if (assets == null)
            {
                throw new InvalidOperationException($"[{filePath}] load failed");
            }

            foreach (Object asset in assets)
            {
                if (asset.name.Equals(subAssetName))
                {
                    return (asset as T, Disposable.Empty);
                }
            }

            return (null, Disposable.Empty);
        }

        public UniTask<(T, IDisposable)> LoadAssetAsync<T>(string bundlePath, string assetName)
            where T : Object
        {
            return UniTask.FromResult(LoadAsset<T>(bundlePath, assetName));
        }
    }
}
#endif
