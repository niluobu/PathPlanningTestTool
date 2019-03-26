using Framework.MVP.Internal;
using Zenject;

namespace Framework.MVP
{
    public static class InstallerHelper
    {
        public static void BindParentView<T>(this DiContainer diContainer)
        {
            diContainer.BindInterfacesTo<ParentView<T>>().AsSingle();
        }

        public static void BindItemView<T>(this DiContainer diContainer)
        {
            diContainer.BindInterfacesTo<ItemView<T>>().AsTransient();
        }

        public static void BindChildView<T>(this DiContainer diContainer)
        {
            diContainer.BindInterfacesTo<ChildView<T>>().AsTransient();
        }

        public static void BindViewManager(this DiContainer diContainer)
        {
            diContainer.BindInterfacesTo<ViewManager>().AsSingle();
        }
    }
}
