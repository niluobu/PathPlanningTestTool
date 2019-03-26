namespace Framework.Bind
{
    public interface IContainerFactory
    {
        IContainer<TContent, TRootBinder> Create<TContent, TRootBinder>()
            where TRootBinder : IBinder;
        IContainer<TContent, TRootBinder> Create<TContent, TRootBinder, TExtraBinder>()
            where TRootBinder : IBinder
            where TExtraBinder : IBinder;
    }
}
