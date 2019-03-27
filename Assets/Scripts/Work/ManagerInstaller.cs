using Framework.Profile.Internal;

namespace Project.Work
{
    public class ManagerInstaller : Zenject.Installer<ManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<DrawManager>().AsSingle();
            Container.BindInterfacesTo<PolygonStorer>().AsSingle();
            Container.BindInterfacesTo<Profile<PolygonSceneConfig>>().AsSingle();
        }
    }
}

