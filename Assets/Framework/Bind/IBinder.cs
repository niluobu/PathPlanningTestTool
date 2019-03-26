using UnityEngine;

namespace Framework.Bind
{
    public interface IBinder
    {
        bool LeafNode { get; }
        void Bind(GameObject gameObject);
        void Unbind();
    }
}
