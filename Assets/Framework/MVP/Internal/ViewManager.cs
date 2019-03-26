using System;
using System.Collections.Generic;
using Zenject;

namespace Framework.MVP.Internal
{
    internal class ViewManager : IInitializable, IDisposable
    {
        private readonly DiContainer _diContainer;
        private readonly List<IParentView> _views;

        public ViewManager(DiContainer diContainer, List<IParentView> views)
        {
            _diContainer = diContainer;
            _views = views;
        }

        public void Initialize()
        {
            _diContainer.ResolveAll<IPresenter>();
        }

        public void Dispose()
        {
            foreach (IParentView view in _views)
            {
                view.Destroy();
            }

            _views.Clear();
        }
    }
}
