using System;
using System.Collections.Generic;
using System.IO;
using UniRx;
using UniRx.Async;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Framework.ResourceManage
{
    internal class AssetBundleManager : IAssetBundleManager, IDisposable
    {
        private readonly string _rootPath;
        private readonly IAssetBundleDependency _dependency;
        private readonly Dictionary<string, IBundle> _bundles = new Dictionary<string, IBundle>();
        private readonly List<IBundle> _dependentBundles = new List<IBundle>();
        private readonly StaticMemoryPool<List<UniTask>> _taskListPool
            = new StaticMemoryPool<List<UniTask>>();

        public AssetBundleManager(string rootPath, IAssetBundleDependency dependency)
        {
            _rootPath = rootPath;
            _dependency = dependency;
        }

        public void Dispose()
        {
            _taskListPool.Dispose();
        }

        public (T, IDisposable) LoadAsset<T>(string bundlePath, string assetName) where T : Object
        {
            IBundle bundle = GetBundle(bundlePath);
            IDisposable disposable = LoadBundle(bundlePath);
            return (bundle.AssetBundle.LoadAsset<T>(assetName), disposable);
        }

        public (T, IDisposable) LoadSubAsset<T>(string bundlePath,
            string assetName, string subAssetName)
            where T : Object
        {
            if (string.IsNullOrEmpty(subAssetName))
            {
                return LoadAsset<T>(bundlePath, assetName);
            }

            IBundle bundle = GetBundle(bundlePath);
            IDisposable disposable = LoadBundle(bundlePath);
            T[] assets = bundle.AssetBundle.LoadAssetWithSubAssets<T>(assetName);
            T ret = null;
            foreach (T asset in assets)
            {
                if (asset.name.Equals(subAssetName))
                {
                    ret = asset;
                    break;
                }
            }

            return (ret, disposable);
        }

        public async UniTask<(T, IDisposable)> LoadAssetAsync<T>(
            string bundlePath, string assetName) where T : Object
        {
            IBundle bundle = GetBundle(bundlePath);
            IDisposable disposable = await LoadBundleAsync(bundlePath);
            AssetBundleRequest request = bundle.AssetBundle.LoadAssetAsync<T>(assetName);
            await request;
            return (request.asset as T, disposable);
        }

        public IDisposable LoadBundle(string path)
        {
            List<IBundle> dependentBundles = GetBundleWithDependencies(path);
            IDisposable disposable = GetBundlesReference(dependentBundles);
            foreach (IBundle bundle in dependentBundles)
            {
                bundle.Load();
            }

            dependentBundles.Clear();
            return disposable;
        }

        public async UniTask<IDisposable> LoadBundleAsync(string path)
        {
            List<IBundle> dependentBundles = GetBundleWithDependencies(path);
            IDisposable disposable = GetBundlesReference(dependentBundles);

            List<UniTask> tasks = _taskListPool.Spawn();
            foreach (IBundle bundle in dependentBundles)
            {
                tasks.Add(bundle.LoadAsync());
            }

            dependentBundles.Clear();
            await UniTask.WhenAll(tasks);

            tasks.Clear();
            _taskListPool.Despawn(tasks);
            return disposable;
        }

        private List<IBundle> GetBundleWithDependencies(string path)
        {
            IBundle bundle = GetBundle(path);
            _dependentBundles.Add(bundle);

            string[] dependencies = _dependency.GetDependencies(path);
            if (dependencies?.Length > 0)
            {
                foreach (string dependency in dependencies)
                {
                    _dependentBundles.Add(GetBundle(dependency));
                }
            }

            return _dependentBundles;
        }

        private IDisposable GetBundlesReference(List<IBundle> bundles)
        {
            BundleReferenceCollection bundleReferenceCollection = BundleReferenceCollection.Pool.Spawn();
            foreach (IBundle bundle in bundles)
            {
                bundleReferenceCollection.Disposables.Add(bundle.GetReference());
            }

            return bundleReferenceCollection;
        }

        private IBundle GetBundle(string path)
        {
            if (!_bundles.TryGetValue(path, out IBundle bundle))
            {
                bundle = CreateBundle(path);
                _bundles[path] = bundle;
            }

            return bundle;
        }

        private IBundle CreateBundle(string path) => new Bundle(Path.Combine(_rootPath, path));
    }
}
