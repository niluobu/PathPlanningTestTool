namespace Framework.MVP.Internal
{
    internal class InputField : Control<UnityEngine.UI.InputField>, IInputField
    {
        public string Text
        {
            get => Component.text;
            set => Component.text = value;
        }
    }
}
