using Framework.Bind;
using UnityEngine;

namespace Framework.MVP.Internal
{
    public class StaticTextBinder : IBinder
    {
        public bool LeafNode => true;

        public void Bind(GameObject gameObject)
        {

        }

        public void Unbind() { }
    }
}
