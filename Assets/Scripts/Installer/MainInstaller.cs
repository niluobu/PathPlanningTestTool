using Project.Work;

namespace Project.Main
{
    public class MainInstaller : Zenject.MonoInstaller
    {
        //public Transform WidgetRoot;

        public override void InstallBindings()
        {
            ManagerInstaller.Install(Container);

        }

        //private void BindUi()
        //{
        //    Container.BindInterfacesTo<SceneHierarchyWidgetsLoader>()
        //        .AsSingle()
        //        .WithArguments(WidgetRoot);
        //    Container.BindViewManager();
        //    Container.BindContainerFactory();
        //    MvpInstaller.Install(Container);

        //    Container.BindInterfacesTo<EditorPresenter>().AsSingle();
        //    Container.BindParentView<EditorWidgets>();
        //}
    }

}
