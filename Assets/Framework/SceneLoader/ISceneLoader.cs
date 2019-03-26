using System;
using Zenject;

namespace Framework.SceneLoader
{
    public interface ISceneLoader
    {
        void Load(string sceneName, Action<DiContainer> extraBindingCallback);
    }
}
