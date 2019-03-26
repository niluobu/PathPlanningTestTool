using System;
using Zenject;

namespace Framework.ResourceManage
{
    internal class BundleReference : IPoolable<IReference>, IDisposable
    {
        public static PoolableStaticMemoryPool<IReference, BundleReference> Pool
            = new PoolableStaticMemoryPool<IReference, BundleReference>();

        private IReference _reference;

        public void Dispose()
        {
            Pool.Despawn(this);
        }

        public void OnDespawned()
        {
            _reference.Release();
            _reference = null;
        }

        public void OnSpawned(IReference reference)
        {
            _reference = reference;
            _reference.Retain();
        }
    }
}
