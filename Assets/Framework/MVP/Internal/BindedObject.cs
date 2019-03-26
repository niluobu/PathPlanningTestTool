using Framework.Bind;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Framework.MVP.Internal
{
    internal class BindedObject : IBinder
    {
        private RectTransform _rectTransform;
        private LayoutGroup _layoutGroup = null;

        public GameObject GameObject { get; private set; }

        public bool Visible
        {
            get => GameObject?.activeSelf ?? false;
            set => GameObject?.SetActive(value);
        }

        public Vector3 WorldPosition => _rectTransform.position;

        public Vector3 AnchoredPosition
        {
            get => _rectTransform.anchoredPosition;
            set => _rectTransform.anchoredPosition = value;
        }

        public virtual bool LeafNode => true;

        public virtual void Bind(GameObject gameObject)
        {
            GameObject = gameObject;
            _rectTransform = GameObject.GetComponent<RectTransform>();
        }

        public virtual void Unbind()
        {
            GameObject = null;
            _rectTransform = null;
        }

        public bool IsBinded() => GameObject != null;

        public virtual void Destroy()
        {
            Object.Destroy(GameObject);
            Unbind();
        }

        public void SetAsLastSibling() => GameObject.transform.SetAsLastSibling();

        public void SetScreenPoint(Vector3 screenPoint, Camera camera)
        {
            if (GameObject.transform is RectTransform rectTransform)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform,
                    screenPoint, camera, out Vector2 localPoint);
                rectTransform.localPosition = localPoint;
            }
        }

        public Vector3 GetScreenPoint(Camera camera) => camera.WorldToScreenPoint(WorldPosition);

        public void GetScreenCorners(Vector3[] fourCornersArray, Camera camera)
        {
            if (_layoutGroup != null || (_layoutGroup = GameObject.GetComponent<LayoutGroup>()) != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            }
            _rectTransform.GetWorldCorners(fourCornersArray);
            for (int i = 0; i < 4; ++i)
            {
                fourCornersArray[i] = camera.WorldToScreenPoint(fourCornersArray[i]);
            }
        }

        public void ForceLayoutImmediate()
        {
            if (_layoutGroup != null || (_layoutGroup = GameObject.GetComponent<LayoutGroup>()) != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            }
        }
    }
}
