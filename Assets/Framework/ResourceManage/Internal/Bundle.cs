using System;
using System.Threading;
using UniRx.Async;
using UnityEngine;

namespace Framework.ResourceManage
{
    internal class Bundle : IBundle, IReference
    {
        private const float UnloadDelayInSeconds = 15f;

        private readonly string _path;
        private UniTask? _loadTask;
        private CancellationTokenSource _cancellationTokenSource;
        private int _reference;

        public Bundle(string path) => _path = path;

        public AssetBundle AssetBundle { get; private set; }

        public void Load()
        {
            if (_loadTask != null)
            {
                throw new InvalidOperationException();
            }

            if (AssetBundle != null)
            {
                StopUnload();
                return;
            }

            AssetBundle = AssetBundle.LoadFromFile(_path);
        }

        public UniTask LoadAsync()
        {
            if (AssetBundle != null)
            {
                StopUnload();
                return UniTask.CompletedTask;
            }

            if (_loadTask == null)
            {
                _loadTask = StartLoad();
            }

            return _loadTask.Value;
        }

        public IDisposable GetReference() => BundleReference.Pool.Spawn(this);

        public void Retain() => _reference++;

        public void Release()
        {
            _reference--;
            if (_reference < 0)
            {
                throw new InvalidOperationException();
            }
            if (0 == _reference)
            {
                StartUnload();
            }
        }

        private async UniTask StartLoad()
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(_path);
            await request;
            AssetBundle = request.assetBundle;
        }

        private async void StartUnload()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                await UniTask.Delay(TimeSpan.FromSeconds(UnloadDelayInSeconds),
                    true, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            _loadTask = null;
            AssetBundle.Unload(false);
            AssetBundle = null;
        }

        private void StopUnload()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
