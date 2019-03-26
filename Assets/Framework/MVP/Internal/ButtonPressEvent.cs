using System;
using UniRx;

namespace Framework.MVP.Internal
{
    public class ButtonPressEvent : IButtonPressEvent, IDisposable
    {
        public Subject<ButtonPreference> Subject { get; } = new Subject<ButtonPreference>();

        public IDisposable Subscribe(IObserver<ButtonPreference> observer)
        {
            return Subject.Subscribe(observer);
        }

        public void Dispose()
        {
            Subject.Dispose();
        }
    }
}
