using System;
using UniRx;

namespace Framework.MVP.Internal
{
    internal class ViewSubject : IParentViewObservable, IDisposable
    {
        public Subject<Unit> LoadSubject { get; } = new Subject<Unit>();
        public Subject<Unit> UnloadSubject { get; } = new Subject<Unit>();
        public Subject<Unit> ShowSubject { get; } = new Subject<Unit>();
        public Subject<Unit> HideSubject { get; } = new Subject<Unit>();

        public IObservable<Unit> Load => LoadSubject;
        public IObservable<Unit> Unload => UnloadSubject;
        public IObservable<Unit> Show => ShowSubject;
        public IObservable<Unit> Hide => HideSubject;

        public void Dispose()
        {
            LoadSubject.Dispose();
            UnloadSubject.Dispose();
            ShowSubject.Dispose();
            HideSubject.Dispose();
        }
    }
}
