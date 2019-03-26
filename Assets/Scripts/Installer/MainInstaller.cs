using Framework.Bind;
using Framework.MVP;
using Project.Common;
using Project.Work;
using UnityEngine;

namespace Project.Main
{
    public class MainInstaller : Zenject.MonoInstaller
    {
        public Transform WidgetRoot;

        public override void InstallBindings()
        {
            ManagerInstaller.Install(Container);
        }

        private void BindUi()
        {
            Container.BindInterfacesTo<SceneHierarchyWidgetsLoader>()
                .AsSingle()
                .WithArguments(WidgetRoot);
            Container.BindViewManager();
            Container.BindContainerFactory();
            MvpInstaller.Install(Container);

            Container.BindInterfacesTo<EditorPresenter>().AsSingle();
            Container.BindParentView<EditorWidgets>();
        }
    }

}
