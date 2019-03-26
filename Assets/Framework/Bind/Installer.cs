using Zenject;

namespace Framework.Bind
{
    public static class Installer
    {
        public static void BindContainerFactory(this DiContainer container)
        {
            container.BindInterfacesTo<ContainerFactory>().AsSingle();
        }
    }
}
