using Project.Work;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Main
{
    public class MainInstaller : Zenject.MonoInstaller
    {
        //public Transform WidgetRoot;
        [SerializeField] private Image _drawPanel;

        public override void InstallBindings()
        {
            BindSetting<ProjectSetting>();

            Container.BindInterfacesTo<DrawPanelPreference>()
                .AsSingle()
                .WithArguments(_drawPanel);

            ManagerInstaller.Install(Container);
        }

        private void BindSetting<T>() where T : ScriptableObject
        {
            T setting = Resources.Load<T>(typeof(T).Name);
            Container.BindInstance(setting);
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
