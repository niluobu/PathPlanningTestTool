using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.MVP
{
    public interface IControl : IUiElement
    {
        bool Visible { get; set; }
    }

    public interface IControl<out T> : IControl
    {
        T Component { get; }
    }

    public interface IPanel : IControl { }

    public interface IButton : IControl<UnityEngine.UI.Button>
    {
        IImage Image { get; }
        IObservable<Unit> Clicked { get; }
    }

    public interface IImage : IControl<UnityEngine.UI.Image>
    {
        Sprite Sprite { get; set; }
        Color Color { get; set; }
        float FillAmount { get; set; }
        bool GrayScale { set; }
        bool Hightlight { set; }
        IObservable<PointerEventData> PointerDownAsObservable { get; }
    }

    public interface IText : IControl
    {
        string Text { get; set; }
        Color Color { set; get; }
    }

    public interface IInputField : IControl<UnityEngine.UI.InputField>
    {
        string Text { get; set; }
    }

    public interface IParticle : IControl
    {
        void Play();
    }

    public interface ISlider : IControl<UnityEngine.UI.Slider>
    {
        float SliderValue { get; set; }
    }

    public interface IToggle : IControl<UnityEngine.UI.Toggle>
    {
        bool On { get; set; }
        IObservable<bool> OnAsObservable { get; }
    }

    public interface IGroup<out TControl> where TControl : IControl
    {
        IReadOnlyList<TControl> Controls { get; }
    }

    public interface IScrollRect : IControl<UnityEngine.UI.ScrollRect>
    {
        float HorizontalNormalizedPosition { get; set; }
        float VerticalNormalizedPosition { get; set; }
    }
}
