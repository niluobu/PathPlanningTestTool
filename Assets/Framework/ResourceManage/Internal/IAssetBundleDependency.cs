namespace Framework.ResourceManage
{
    internal interface IAssetBundleDependency
    {
        string[] GetDependencies(string bundlePath);
    }
}
