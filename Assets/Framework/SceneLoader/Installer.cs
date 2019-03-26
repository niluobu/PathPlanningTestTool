using Zenject;

namespace Framework.SceneLoader
{
    public static class Installer
    {
        public static void InstallSceneLoader(DiContainer container)
        {
            container.BindInterfacesTo<SceneLoader>().AsSingle();
        }
    }
}
