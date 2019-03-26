using System;
using UniRx.Async;
using UnityEngine;

namespace Framework.ResourceManage
{
    internal interface IBundle
    {
        AssetBundle AssetBundle { get; }
        void Load();
        UniTask LoadAsync();
        IDisposable GetReference();
    }
}
