using TMPro;
using UnityEngine;

namespace Framework.MVP.Internal
{
    internal class Text : Control<RectTransform>, IText
    {
        private UnityEngine.UI.Text _text;
        private TextMeshProUGUI _textMesh;
        private string _key;

        string IText.Text
        {
            get => GetTextFromComponent();
            set => SetTextForComponent(value);
        }

        public Color Color
        {
            set
            {
                if (_text != null)
                {
                    _text.color = value;
                }

                if (_textMesh != null)
                {
                    _textMesh.color = value;
                }
            }
            get
            {
                if (_text != null)
                {
                    return _text.color;
                }
                if (_textMesh != null)
                {
                    return _textMesh.color;
                }
                return Color.white;
            }
        }

        public override void Bind(GameObject gameObject)
        {
            base.Bind(gameObject);

            _text = gameObject.GetComponent<UnityEngine.UI.Text>();
            _textMesh = gameObject.GetComponent<TextMeshProUGUI>();

            _key = GetTextFromComponent();

            TextPreference textPreference = gameObject.GetComponent<TextPreference>();
            if (textPreference != null)
            {
                _key = textPreference.LocalizationKey;
            }
        }

        public override void Unbind()
        {
            base.Unbind();

            _text = null;
            _textMesh = null;
        }

        private string GetTextFromComponent()
        {
            if (_text != null)
            {
                return _text.text;
            }

            if (_textMesh != null)
            {
                return _textMesh.text;
            }

            return string.Empty;
        }

        private void SetTextForComponent(string text)
        {
            if (_text != null)
            {
                _text.text = text;
            }

            if (_textMesh != null)
            {
                _textMesh.text = text;
            }
        }
    }
}
