using System;
using System.Collections.Generic;
using Zenject;

namespace Framework.ResourceManage
{
    internal class BundleReferenceCollection : IPoolable, IDisposable
    {
        public static PoolableStaticMemoryPool<BundleReferenceCollection> Pool { get; }
            = new PoolableStaticMemoryPool<BundleReferenceCollection>();

        public List<IDisposable> Disposables { get; } = new List<IDisposable>();

        public void OnDespawned()
        {
            foreach (IDisposable disposable in Disposables)
            {
                disposable.Dispose();
            }
            Disposables.Clear();
        }

        public void OnSpawned() { }

        public void Dispose()
        {
            Pool.Despawn(this);
        }
    }
}
