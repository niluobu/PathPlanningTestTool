using Framework.MVP.Internal;
using Zenject;

namespace Framework.MVP
{
    public class MvpInstaller : Installer<MvpInstaller>
    {
        public override void InstallBindings()
        {
            Install();
            InstallUiElements();
        }

        private void Install()
        {
            Container.Bind<StaticTextBinder>().AsTransient();
            Container.Bind<ViewSubject>().AsTransient();
            Container.BindInterfacesTo<ImageShareMaterial>().AsSingle();
            Container.BindInterfacesAndSelfTo<ButtonPressEvent>().AsSingle();
            Container.Bind<ViewAnimationProvider>().AsSingle();
        }

        private void InstallUiElements()
        {
            InstallUiClass<IPanel, Panel>();
            InstallUiClass<IButton, Button>();
            InstallUiClass<IImage, Image>();
            InstallUiClass<IText, Text>();
            InstallUiClass<IInputField, InputField>();
            InstallUiClass<IParticle, Particle>();
            InstallUiClass<ISlider, Slider>();
            InstallUiClass<IToggle, Toggle>();
            InstallUiClass<IScrollRect, ScrollRect>();
        }

        private void InstallUiClass<TInterface, TImplement>()
            where TInterface : IControl
            where TImplement : TInterface
        {
            Container.BindInterfacesAndSelfTo<TImplement>().AsTransient();
            Container.Bind<IGroup<TInterface>>().To<Group<TInterface>>().AsTransient();
        }
    }
}
