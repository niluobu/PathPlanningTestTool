using Framework.Bind;
using UniRx;

namespace Framework.MVP.Internal
{
    internal class ChildView<T> : BaseView<T>, IChildView<T>, IBinder
    {
        public IChildViewObservable ViewObservable => ViewSubject;

        public override void Show()
        {
            Container.Root.Visible = true;
            if (Container.Root.GameObject.activeInHierarchy)
            {
                ViewSubject.ShowSubject.OnNext(Unit.Default);
            }
        }

        public override void Hide()
        {
            Container.Root.Visible = false;
            ViewSubject.HideSubject.OnNext(Unit.Default);
            HideDisposables.Clear();
        }
    }
}
