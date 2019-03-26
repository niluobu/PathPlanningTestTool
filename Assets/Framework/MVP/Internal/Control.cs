using UnityEngine;

namespace Framework.MVP.Internal
{
    internal class Control<T> : BindedObject where T : class
    {
        private readonly Animator _animator = new Animator();

        public IAnimator Animator => _animator;

        public T Component { get; private set; }

        public override void Bind(GameObject gameObject)
        {
            base.Bind(gameObject);
            Component = gameObject.GetComponent<T>();
            _animator.Bind(gameObject);
        }

        public override void Unbind()
        {
            base.Unbind();
            Component = null;
            _animator.Unbind();
        }
    }
}
