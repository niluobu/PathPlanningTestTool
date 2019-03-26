using UnityEngine;

namespace Framework.MVP
{
    public interface IUiElement
    {
        GameObject GameObject { get; }
        Vector3 WorldPosition { get; }
        Vector3 AnchoredPosition { get; set; }
        IAnimator Animator { get; }
        void SetAsLastSibling();
        void SetScreenPoint(Vector3 screenPoint, Camera camera);
        Vector3 GetScreenPoint(Camera camera);
        void GetScreenCorners(Vector3[] fourCornersArray, Camera camera);
        void ForceLayoutImmediate();
    }
}
