using Zenject;

namespace Framework.Bind
{
    internal class ContainerFactory : IContainerFactory
    {
        [Inject] private readonly DiContainer _container = null;

        public IContainer<TContent, TRootBinder> Create<TContent, TRootBinder>()
            where TRootBinder : IBinder
        {
            return _container.InstantiateExplicit<Container<TContent, TRootBinder>>(
                InjectUtil.CreateArgListExplicit<IBinder>(null));
        }

        public IContainer<TContent, TRootBinder> Create<TContent, TRootBinder, TExtraBinder>()
            where TRootBinder : IBinder
            where TExtraBinder : IBinder
        {
            IBinder binder = _container.Resolve<TExtraBinder>();
            return _container.InstantiateExplicit<Container<TContent, TRootBinder>>(
                InjectUtil.CreateArgListExplicit(binder));
        }
    }
}
