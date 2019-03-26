using UnityEngine;

namespace Framework.ResourceManage
{
    internal class AssetBundleDependency : IAssetBundleDependency, Zenject.IInitializable
    {
        private readonly string _manifestBundlePath;
        private AssetBundleManifest _manifest;

        public AssetBundleDependency(string manifestBundlePath) => _manifestBundlePath = manifestBundlePath;

        public string[] GetDependencies(string bundlePath) => _manifest.GetAllDependencies(bundlePath);

        public void Initialize()
        {
            AssetBundle bundle = AssetBundle.LoadFromFile(_manifestBundlePath);
            _manifest = bundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
            bundle.Unload(false);
        }
    }
}
