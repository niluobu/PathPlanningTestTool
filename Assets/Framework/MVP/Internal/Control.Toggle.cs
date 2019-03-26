using System;
using UniRx;

namespace Framework.MVP.Internal
{
    internal class Toggle : Control<UnityEngine.UI.Toggle>, IToggle
    {
        public bool On
        {
            get => Component.isOn;
            set => Component.isOn = value;
        }

        public IObservable<bool> OnAsObservable => Component.OnValueChangedAsObservable();
    }
}
