using System.Collections.Generic;
using Framework.Bind;
using UnityEngine;
using Zenject;

namespace Framework.MVP.Internal
{
    public class Group<TControl> : IBinder, IGroup<TControl> where TControl : IControl
    {
        private readonly DiContainer _diContainer;
        private readonly List<TControl> _controls = new List<TControl>();

        public Group(DiContainer diContainer) => _diContainer = diContainer;

        public IReadOnlyList<TControl> Controls => _controls;

        public bool LeafNode => true;

        public void Bind(GameObject gameObject)
        {
            TControl control = _diContainer.Resolve<TControl>();
            _controls.Add(control);
            (control as IBinder)?.Bind(gameObject);
        }

        public void Unbind()
        {
            foreach (TControl control in _controls)
            {
                (control as IBinder)?.Unbind();
            }
            _controls.Clear();
        }
    }
}
