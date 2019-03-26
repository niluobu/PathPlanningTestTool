using System;

namespace Framework.MVP
{
    public interface IPresenter { }

    public interface IItemPresenter<in TWidgets> : IDisposable
    {
        void OnShow(IView<TWidgets> view, object context);
    }
}
