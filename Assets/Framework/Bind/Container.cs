using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace Framework.Bind
{
    internal class Container<TContent, TRootBinder> : IContainer<TContent, TRootBinder>
        where TRootBinder : IBinder
    {
        private readonly IBinder _extraBinder;
        private readonly Dictionary<string, IBinder> _binders;

        public Container(DiContainer container, IBinder extraBinder)
        {
            Content = container.Instantiate<TContent>();
            Root = container.Resolve<TRootBinder>();
            _extraBinder = extraBinder;

            _binders = new Dictionary<string, IBinder>();
            foreach (PropertyInfo property in typeof(TContent).GetProperties())
            {
                if (container.Resolve(property.PropertyType) is IBinder binder)
                {
                    property.SetValue(Content, binder);
                    _binders[property.Name] = binder;
                }
            }
        }

        public TContent Content { get; }

        public TRootBinder Root { get; }

        public bool LeafNode => false;

        public void Bind(GameObject gameObject)
        {
            Root.Bind(gameObject);
            LoopChildObject(gameObject);
        }

        public void Unbind()
        {
            foreach (IBinder binder in _binders.Values)
            {
                binder.Unbind();
            }

            Root.Unbind();
        }

        private void LoopChildObject(GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                BindAndLoopChild(child.gameObject);
            }
        }

        private void BindAndLoopChild(GameObject gameObject)
        {
            if (_binders.TryGetValue(gameObject.name, out IBinder binder))
            {
                binder.Bind(gameObject);
                if (!binder.LeafNode)
                {
                    return;
                }
            }
            else
            {
                _extraBinder?.Bind(gameObject);
            }

            LoopChildObject(gameObject);
        }
    }
}
