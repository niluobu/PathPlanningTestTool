using System;
using System.Threading;
using UniRx.Async;

namespace Framework.MVP
{
    public class ViewAnimationProvider
    {
        public Func<IUiElement, Type, CancellationToken, UniTask> ShowAnimFunc { get; set; }
        public Func<IUiElement, Type, CancellationToken, UniTask> HideAnimFunc { get; set; }
    }
}
