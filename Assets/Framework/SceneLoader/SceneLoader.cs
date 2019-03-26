using System;
using UniRx;
using UniRx.Async;
using UnityEngine;
using Zenject;

namespace Framework.SceneLoader
{
    internal class SceneLoader : ISceneLoader
    {
        private readonly float _duration = 0.5f;
        private readonly ZenjectSceneLoader _zenjectSceneLoader;
        private bool _loading = false;

        public SceneLoader(ZenjectSceneLoader zenjectSceneLoader)
        {
            _zenjectSceneLoader = zenjectSceneLoader;
        }

        public async void Load(string sceneName, Action<DiContainer> extraBindingCallback)
        {
            if (_loading)
            {
                return;
            }
            _loading = true;

            Transition transition = Transition.Create();
            transition.Begin(_duration, Color.black);
            await Observable.Timer(TimeSpan.FromSeconds(_duration));

            _zenjectSceneLoader.LoadScene(sceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single,
                container =>
            {
                extraBindingCallback?.Invoke(container);
            });

            transition.End(_duration);
            _loading = false;
        }
    }
}
