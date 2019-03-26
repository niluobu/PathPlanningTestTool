using System.Collections.Generic;
using UnityEngine;

namespace Framework.MVP.Internal
{
    internal interface IImageShareMaterial
    {
        Material Get(bool disable, bool highlight);
    }

    internal class ImageShareMaterial : IImageShareMaterial
    {
        private Dictionary<int, Material> _materials = new Dictionary<int, Material>();

        public Material Get(bool disable, bool highlight)
        {
            int flag = 0;
            if (disable)
            {
                flag = flag | 0x1;
            }
            if (highlight)
            {
                flag = flag | 0x2;
            }

            if (_materials.TryGetValue(flag, out Material material))
            {
                return material;
            }

            return Create(flag, disable, highlight);
        }

        private Material Create(int flag, bool disable, bool highlight)
        {
            var material = new Material(Shader.Find("Framework/UiGrayAndHighlight"));
            material.SetFloat("_DisableFlag", disable ? 1 : 0);
            material.SetFloat("_HighlightFlag", highlight ? 1 : 0);
            _materials[flag] = material;
            return material;
        }
    }
}
