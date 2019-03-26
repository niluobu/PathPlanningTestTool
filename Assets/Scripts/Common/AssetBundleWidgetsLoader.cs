using Framework.MVP;
using Framework.ResourceManage;
using System;
using UniRx.Async;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Project.Common
{
    public interface IWidgetsSettingProvider
    {
        (string bundlePath, string assetName, string widgetParent) GetSetting<T>();
    }

    public class AssetBundleWidgetsLoader : IWidgetsAssetLoader
    {
        private readonly IAssetBundleManager _assetBundleManager;
        private readonly Transform _rooTransform;
        private readonly IWidgetsSettingProvider _settingProvider;

        public AssetBundleWidgetsLoader(IAssetBundleManager assetBundleManager,
            Transform rooTransform,
            IWidgetsSettingProvider settingProvider)
        {
            _assetBundleManager = assetBundleManager;
            _rooTransform = rooTransform;
            _settingProvider = settingProvider;
        }

        public UniTask<GameObject> Load<T>()
        {
            (string bundlePath, string assetName, string widgetParent) =
                _settingProvider.GetSetting<T>();
            (GameObject prefab, IDisposable disposable) =
                _assetBundleManager.LoadAsset<GameObject>(bundlePath, assetName);

            using (disposable)
            {
                Transform parent = _rooTransform.Find(widgetParent) ?? _rooTransform;
                GameObject gameObject = Object.Instantiate(prefab, parent);
                return UniTask.FromResult(gameObject);
            }
        }
    }
}
