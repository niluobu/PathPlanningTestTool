using System;
using System.Threading;
using UniRx;
using UniRx.Async;
using UnityEngine;
using Zenject;

namespace Framework.MVP.Internal
{
    internal interface IParentView
    {
        void Destroy();
    }

    internal class ParentView<T> : BaseView<T>, IParentView, IParentView<T>
    {
        [Inject] private readonly IWidgetsAssetLoader _loader = null;
        [Inject] private readonly ViewAnimationProvider _animationProvider = null;

        private bool _hiding;
        private CancellationTokenSource _cancellationTokenSource;

        public IParentViewObservable ViewObservable => ViewSubject;

        public async UniTask Load(bool forceShow)
        {
            if (!Container.Root.IsBinded())
            {
                GameObject widgets = await _loader.Load<T>();
                Container.Bind(widgets);
                ViewSubject.LoadSubject.OnNext(Unit.Default);
            }

            if (forceShow || Container.Root.Visible)
            {
                Visible = true;
            }
        }

        public override async void Show()
        {
            if (_cancellationTokenSource != null)
            {
                return;
            }

            Container.Root.Visible = true;
            Root.SetAsLastSibling();
            ViewSubject.ShowSubject.OnNext(Unit.Default);

            if (_animationProvider.ShowAnimFunc != null)
            {
                await WaitAnimation(
                    token => _animationProvider.ShowAnimFunc(Root, typeof(T), token));
            }
        }

        public override async void Hide()
        {
            if (_hiding)
            {
                return;
            }

            _hiding = true;

            if (_animationProvider.HideAnimFunc != null)
            {
                await WaitAnimation(
                    token => _animationProvider.HideAnimFunc(Root, typeof(T), token),
                    OnHideAnimFinish);
            }
            else
            {
                OnHideAnimFinish();
            }
        }

        private void OnHideAnimFinish()
        {
            Container.Root.Visible = false;
            ViewSubject.HideSubject.OnNext(Unit.Default);
            HideDisposables.Clear();
            _hiding = false;
        }

        public void Unload()
        {
            ViewSubject.UnloadSubject.OnNext(Unit.Default);
            HideDisposables.Clear();
            UnloadDisposables.Clear();
            Container.Root.Destroy();
            Container.Unbind();
        }

        public void Destroy()
        {
            Unload();
            ViewSubject.Dispose();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        private async UniTask WaitAnimation(Func<CancellationToken, UniTask> func,
            Action postAction = null)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await func(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            postAction?.Invoke();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
