using Framework.Bind;
using UnityEngine;

namespace Framework.MVP.Internal
{
    public class Animator : IAnimator, IBinder
    {
        private UnityEngine.Animator _animator;

        public bool LeafNode => true;

        public void Bind(GameObject gameObject)
            => _animator = gameObject.GetComponent<UnityEngine.Animator>();

        public void Unbind() => _animator = null;

        public void Play(string stateName) => _animator.Play(stateName);
    }
}
