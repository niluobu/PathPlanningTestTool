using System.Collections.Generic;
using Framework.Bind;
using UnityEngine;
using Zenject;

namespace Framework.MVP.Internal
{
    internal class ItemViewItem<T> : BaseView<T>, IView<T>, IBinder
    {
        private IItemPresenter<T> _presenter;

        public override void Show()
        {
            Container.Root.Visible = true;
        }

        public override void Hide()
        {
            Container.Root.Visible = false;
            HideDisposables.Clear();
            _presenter?.Dispose();
            _presenter = null;
        }

        public void Show(IItemPresenter<T> presenter, object context)
        {
            Show();
            _presenter = presenter;
            _presenter?.OnShow(this, context);
        }
    }

    internal class ItemView<T> : BindedObject, IBinder, IItemView<T>
    {
        private readonly DiContainer _container;
        private readonly List<ItemViewItem<T>> _items = new List<ItemViewItem<T>>();

        public ItemView(DiContainer container) => _container = container;

        public override bool LeafNode => false;

        public override void Bind(GameObject gameObject)
        {
            base.Bind(gameObject);
            Visible = false;
        }

        public override void Unbind()
        {
            base.Unbind();
            foreach (ItemViewItem<T> item in _items)
            {
                item.Unbind();
            }
            _items.Clear();
        }

        public IView<T> Take()
        {
            IView<T> view = TakeOrCreate();
            view.Visible = true;
            return view;
        }

        public void ShowOne<TPresenter>(object context = null) where TPresenter : IItemPresenter<T>
        {
            ItemViewItem<T> view = TakeOrCreate();
            IItemPresenter<T> presenter = _container.Resolve<TPresenter>();
            view.Show(presenter, context);
        }

        private ItemViewItem<T> TakeOrCreate()
        {
            ItemViewItem<T> view = null;
            foreach (ItemViewItem<T> item in _items)
            {
                if (!item.Visible)
                {
                    view = item;
                    break;
                }
            }

            if (view == null)
            {
                view = _container.Instantiate<ItemViewItem<T>>();
                GameObject gameObject = Object.Instantiate(GameObject, GameObject.transform.parent);
                view.Bind(gameObject);
                _items.Add(view);
            }

            view.Root.SetAsLastSibling();
            return view;
        }

        public void HideAll()
        {
            foreach (ItemViewItem<T> item in _items)
            {
                item.Visible = false;
            }
        }
    }
}
