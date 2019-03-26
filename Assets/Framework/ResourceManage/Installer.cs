using System.IO;
using Zenject;

namespace Framework.ResourceManage
{
    public static class Installer
    {
        private const int ExecuteOrder = -10;

        public static void InstallNormal(this DiContainer container, string assetBundleRoot, string manifestBundlePath)
        {
            container.BindInterfacesTo<AssetBundleDependency>()
                .AsSingle()
                .WithArguments(Path.Combine(assetBundleRoot, manifestBundlePath));

            container.BindExecutionOrder<AssetBundleDependency>(ExecuteOrder);

            container.BindInterfacesTo<AssetBundleManager>()
                .AsSingle()
                .WithArguments(assetBundleRoot);
        }

        public static void InstallEditor(this DiContainer container)
        {
#if UNITY_EDITOR
            container.BindInterfacesTo<AssetBundleManagerEditor>().AsSingle();
#endif
        }

        public static void InstallResource(this DiContainer container)
        {
            container.BindInterfacesTo<AssetBundleManagerResource>().AsSingle();
        }
    }
}
