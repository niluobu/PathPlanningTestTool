using System;
using UniRx;

namespace Framework.MVP
{
    public interface IChildViewObservable
    {
        IObservable<Unit> Show { get; }
        IObservable<Unit> Hide { get; }
    }

    public interface IParentViewObservable : IChildViewObservable
    {
        IObservable<Unit> Load { get; }
        IObservable<Unit> Unload { get; }
    }
}
