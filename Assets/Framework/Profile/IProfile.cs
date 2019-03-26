namespace Framework.Profile
{
    public interface IProfile<out T> where T : class, new()
    {
        T Instance { get; }
        void Save();
    }
}
