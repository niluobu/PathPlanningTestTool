namespace Framework.MVP.Internal
{
    internal class ScrollRect : Control<UnityEngine.UI.ScrollRect>, IScrollRect
    {
        public float HorizontalNormalizedPosition
        {
            get => Component.horizontalNormalizedPosition;
            set => Component.horizontalNormalizedPosition = value;
        }

        public float VerticalNormalizedPosition
        {
            get => Component.verticalNormalizedPosition;
            set => Component.verticalNormalizedPosition = value;
        }
    }
}
