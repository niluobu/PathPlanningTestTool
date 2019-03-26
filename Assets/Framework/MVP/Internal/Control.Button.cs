using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace Framework.MVP.Internal
{
    internal class Button : Control<UnityEngine.UI.Button>, IButton
    {
        private const float ClickScale = 1f;
        [Inject] private readonly Image _image = null;
        [Inject] private readonly ButtonPressEvent _buttonPressEvent = null;

        private Vector3 _scale;
        private float _clickScale;
        private ButtonPreference _buttonPreference;

        public IImage Image => _image;

        public IObservable<Unit> Clicked => Component.OnClickAsObservable();

        public override void Bind(GameObject gameObject)
        {
            base.Bind(gameObject);
            _image.Bind(gameObject);

            _image.PointerDownAsObservable.Subscribe(_ => OnPointerDown());
            _image.PointerUpAsObservable.Subscribe(_ => OnPointUp());
            Clicked.Subscribe(_ => OnClicked());

            _scale = GameObject.transform.localScale;

            _buttonPreference = gameObject.GetComponent<ButtonPreference>();
            _clickScale = _buttonPreference?.PressScale ?? ClickScale;
        }

        public override void Unbind()
        {
            base.Unbind();
            _image.Unbind();
            _buttonPreference = null;
        }

        private void OnPointerDown()
        {
            _image.Hightlight = true;
            GameObject.transform.localScale = _scale * _clickScale;
        }

        private void OnPointUp()
        {
            GameObject.transform.localScale = _scale;
            _image.Hightlight = false;
        }

        private void OnClicked()
        {
            _buttonPressEvent.Subject.OnNext(_buttonPreference);
        }
    }
}
