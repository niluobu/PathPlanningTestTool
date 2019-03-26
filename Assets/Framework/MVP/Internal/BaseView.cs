using System;
using System.Collections.Generic;
using Framework.Bind;
using UniRx;
using UnityEngine;
using Zenject;

namespace Framework.MVP.Internal
{
    internal abstract class BaseView
    {
        private readonly CompositeDisposable _disposablesAfterHide = new CompositeDisposable();
        private readonly CompositeDisposable _disposablesAfterUnload = new CompositeDisposable();

        public bool Visible
        {
            get => IsVisible();
            set
            {
                if (value)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        public ICollection<IDisposable> HideDisposables => _disposablesAfterHide;

        public ICollection<IDisposable> UnloadDisposables => _disposablesAfterUnload;

        public IAnimator Animator => null;

        public bool LeafNode => false;

        public abstract bool IsVisible();

        public abstract void Show();

        public abstract void Hide();

        protected ViewSubject ViewSubject { get; } = new ViewSubject();
    }

    internal abstract class BaseView<T> : BaseView
    {
        [Inject] private readonly IContainerFactory _containerFactory = null;
        private IContainer<T, Panel> _container;

        public IUiElement Root => Container.Root;

        public T Widgets => Container.Content;

        protected IContainer<T, Panel> Container
            => _container ?? (_container = _containerFactory.Create<T, Panel, StaticTextBinder>());

        public override bool IsVisible() => Container.Root.Visible;

        public virtual void Bind(GameObject gameObject) => Container.Bind(gameObject);

        public virtual void Unbind()
        {
            Container.Unbind();
            HideDisposables.Clear();
            UnloadDisposables.Clear();
        }
    }
}
