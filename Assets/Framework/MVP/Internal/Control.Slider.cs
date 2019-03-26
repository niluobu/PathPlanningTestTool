namespace Framework.MVP.Internal
{
    internal class Slider : Control<UnityEngine.UI.Slider>, ISlider
    {
        public float SliderValue
        {
            get => Component.value;
            set => Component.value = value;
        }
    }
}
