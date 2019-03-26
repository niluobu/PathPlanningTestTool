using System;
using System.Collections.Generic;
using UniRx.Async;

namespace Framework.MVP
{
    public interface IView<out T>
    {
        bool Visible { get; set; }
        IUiElement Root { get; }
        ICollection<IDisposable> HideDisposables { get; }
        T Widgets { get; }
    }

    public interface IItemView<out T>
    {
        IView<T> Take();
        void ShowOne<TPresenter>(object context = null) where TPresenter : IItemPresenter<T>;
        void HideAll();
    }

    public interface IChildView<out T> : IView<T>
    {
        IChildViewObservable ViewObservable { get; }
    }

    public interface IParentView<out T> : IView<T>
    {
        UniTask Load(bool forceShow = false);
        void Unload();
        ICollection<IDisposable> UnloadDisposables { get; }
        IParentViewObservable ViewObservable { get; }
    }
}
