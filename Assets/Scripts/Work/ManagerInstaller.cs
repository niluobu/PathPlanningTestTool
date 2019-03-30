using Framework.Profile.Internal;

namespace Project.Work
{
    public class ManagerInstaller : Zenject.Installer<ManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GlUtil>().AsSingle();
            Container.BindInterfacesTo<PolygonSceneStorer>().AsSingle();
            Container.BindInterfacesTo<PolygonSceneChecker>().AsSingle();
            Container.BindInterfacesTo<Profile<PolygonSceneConfig>>().AsSingle();
            Container.BindInterfacesTo<RunManager>().AsSingle();
        }
    }
}

