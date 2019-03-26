using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.MVP.Internal
{
    internal class ImageEventHandler<T> : MonoBehaviour, IObservable<T>, IDisposable
    {
        protected Subject<T> Subject { get; private set; }

        public IDisposable Subscribe(IObserver<T> observer) => GetSubject().Subscribe(observer);

        public void Dispose()
        {
            Subject?.Dispose();
            Subject = null;
        }

        private Subject<T> GetSubject()
        {
            Subject = Subject ?? new Subject<T>();
            return Subject;
        }
    }

    internal class PointerDownHandler : ImageEventHandler<PointerEventData>, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData) => Subject?.OnNext(eventData);
    }

    internal class PointerUpHandler : ImageEventHandler<PointerEventData>, IPointerUpHandler
    {
        public void OnPointerUp(PointerEventData eventData) => Subject?.OnNext(eventData);
    }

    internal class Image : Control<UnityEngine.UI.Image>, IImage
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IImageShareMaterial _shareMaterial;

        private bool _disable;
        private bool _highlight;

        public Image(IImageShareMaterial shareMaterial)
        {
            _shareMaterial = shareMaterial;
        }

        public Sprite Sprite
        {
            get => Component.sprite;
            set => Component.sprite = value;
        }

        public Color Color
        {
            get => Component.color;
            set => Component.color = value;
        }

        public float FillAmount
        {
            get => Component.fillAmount;
            set => Component.fillAmount = value;
        }

        public override void Unbind()
        {
            base.Unbind();
            _disposables.Clear();
            _disable = false;
            _highlight = false;
        }

        public bool GrayScale
        {
            set => SetMaterialFlag(value, ref _disable);
        }

        public bool Hightlight
        {
            set => SetMaterialFlag(value, ref _highlight);
        }

        public IObservable<PointerEventData> PointerDownAsObservable
            => GetHandler<PointerDownHandler>();

        public IObservable<PointerEventData> PointerUpAsObservable
            => GetHandler<PointerUpHandler>();

        private T GetHandler<T>() where T : MonoBehaviour, IDisposable
        {
            T component = GameObject.GetComponent<T>();
            if (component == null)
            {
                component = GameObject.AddComponent<T>();
                _disposables.Add(component);
            }

            return component;
        }

        private void SetMaterialFlag(bool value, ref bool flag)
        {
            if (flag == value)
            {
                return;
            }

            flag = value;
            CreateMaterial();
        }

        private void CreateMaterial()
        {
            if (Component == null)
            {
                return;
            }

            Component.material = _shareMaterial.Get(_disable, _highlight);
        }
    }
}
