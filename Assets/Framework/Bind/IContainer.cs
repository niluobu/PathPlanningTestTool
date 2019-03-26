namespace Framework.Bind
{
    public interface IContainer<out TContent, out TRootBinder>
        : IBinder
        where TRootBinder : IBinder
    {
        TContent Content { get; }
        TRootBinder Root { get; }
    }
}
