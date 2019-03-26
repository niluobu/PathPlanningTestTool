using System;
using UniRx;
using UniRx.Async;

namespace Framework.ResourceManage
{
    internal abstract class BasicSimpleAssetBundleManager
    {
        public IDisposable LoadBundle(string path) => Disposable.Empty;

        public UniTask<IDisposable> LoadBundleAsync(string path) => UniTask.FromResult(LoadBundle(path));
    }
}
