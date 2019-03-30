using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Project.Work
{
    public interface IDrawPanelPreference
    {
        Texture2D Texture { get; }
        void Apply();
        void SetTextureBgColor();
        bool MouseIsInDrawPanel(Vector2Int screenPos);
        bool PointAroundIsBgColor(Vector2Int screenPos);
        Vector2Int ConvertToDrawPanelPos(Vector2Int pos);
    }

    public class DrawPanelPreference : IDrawPanelPreference, IInitializable
    {
        private readonly Image _drawPanel;
        private readonly ProjectSetting _projectSetting;

        private Texture2D _texture;
        private Rect _textureRect;

        public Texture2D Texture => _texture;

        public DrawPanelPreference(Image drawPanel,
            ProjectSetting projectSetting)
        {
            _drawPanel = drawPanel;
            _projectSetting = projectSetting;
        }

        public void Initialize()
        {
            _texture = new Texture2D((int)_drawPanel.rectTransform.rect.width, (int)_drawPanel.rectTransform.rect.height);
            _drawPanel.material.SetTexture("_MainTex", _texture);
            SetTextureBgColor();
            SetTextureRect();
        }

        public void Apply()
        {
            _texture.Apply();
            _drawPanel.enabled = false;
            _drawPanel.enabled = true;
        }

        public Vector2Int ConvertToDrawPanelPos(Vector2Int pos)
        {
            Vector2Int cpos = new Vector2Int
            {
                x = pos.x - (int)_textureRect.xMin,
                y = pos.y - (int)_textureRect.yMin
            };
            return cpos;
        }

        public void SetTextureBgColor()
        {
            for (int mip = 0; mip < _texture.mipmapCount; ++mip)
            {
                Color[] cols = _texture.GetPixels(mip);
                for (int i = 0; i < cols.Length; ++i)
                {
                    cols[i] = _projectSetting.TextureBgColor;
                }
                _texture.SetPixels(cols, mip);
            }

            Apply();
        }

        public bool MouseIsInDrawPanel(Vector2Int screenPos)
        {
            if (screenPos.x < _textureRect.xMin || screenPos.x > _textureRect.xMax ||
                screenPos.y < _textureRect.yMin || screenPos.y > _textureRect.yMax)
            {
                return false;
            }

            return true;
        }

        private void SetTextureRect()
        {
            Vector3[] fourCornersArray = new Vector3[4];
            _drawPanel.rectTransform.GetWorldCorners(fourCornersArray);
            fourCornersArray[0] = Camera.main.WorldToScreenPoint(fourCornersArray[0]);
            fourCornersArray[2] = Camera.main.WorldToScreenPoint(fourCornersArray[2]);
            _textureRect = new Rect
            {
                min = new Vector2(fourCornersArray[0].x, fourCornersArray[0].y),
                max = new Vector2(fourCornersArray[2].x, fourCornersArray[2].y)
            };
        }

        public bool PointAroundIsBgColor(Vector2Int screenPos)
        {
            screenPos = ConvertToDrawPanelPos(screenPos);
            return CircleChecker(screenPos, _projectSetting.SEPointCheckRadius);
        }

        public bool CircleChecker(Vector2Int center, int radius)
        {
            int x = 0;
            int y = radius;
            int delta = 3 - 2 * radius;
            if (PointsCheck(center, x, y) == false)
            {
                return false;
            }
            while (x < y)
            {
                if (delta < 0)
                {
                    delta += 2 * x + 3;
                }
                else
                {
                    delta += 2 * (x - y) + 5;
                    --y;
                }

                ++x;
                if (PointsCheck(center, x, y) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool PointsCheck(Vector2Int center, int x, int y)
        {
            if (_texture.GetPixel(center.x + x, center.y + y) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + y, center.y + x) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + y, center.y + (-x)) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + x, center.y + (-y)) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + (-x), center.y + (-y)) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + (-y), center.y + (-x)) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + (-y), center.y + x) == _projectSetting.TextureBgColor &&
                _texture.GetPixel(center.x + (-x), center.y + y) == _projectSetting.TextureBgColor)
            {
                return true;
            }

            return false;
        }
    }
}
