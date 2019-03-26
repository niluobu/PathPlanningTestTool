using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.SceneLoader
{
    public class Transition : MonoBehaviour
    {
        private const string CanvasObjectName = "TransitionCanvas";
        private static GameObject _canvasObject = null;
        private Image _overlayImage = null;
        private bool _ending = false;
        private float _duration = 0f;
        private float _startTime = 0f;
        private Color _fadingColor = Color.black;

        public static Transition Create()
        {
            if (_canvasObject != null)
            {
                throw new InvalidOperationException();
            }

            var gameObject = new GameObject(nameof(Transition));
            var transition = gameObject.AddComponent<Transition>();
            gameObject.transform.SetParent(_canvasObject.transform, false);
            gameObject.transform.SetAsLastSibling();
            return transition;
        }

        private void Awake()
        {
            _canvasObject = new GameObject(CanvasObjectName);
            var canvas = _canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            DontDestroyOnLoad(_canvasObject);
        }

        private void Update()
        {
            InitOverlay();

            float percentage = GetAlphaPercentage();
            _overlayImage.canvasRenderer.SetAlpha(Mathf.InverseLerp(0, 1, percentage));
            if (percentage < 0 && _ending)
            {
                Destroy(_canvasObject);
                _canvasObject = null;
            }
        }

        private float GetAlphaPercentage()
        {
            return GetDeltaTime() / _duration;
        }

        private float GetDeltaTime()
        {
            if (!_ending)
            {
                return Time.realtimeSinceStartup - _startTime;
            }
            else
            {
                return _duration - (Time.realtimeSinceStartup - _startTime);
            }
        }

        private void InitOverlay()
        {
            if (_overlayImage != null)
            {
                return;
            }

            var bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, _fadingColor);
            bgTex.Apply();

            var gameObject = new GameObject();
            _overlayImage = gameObject.AddComponent<Image>();
            var rect = new Rect(0, 0, bgTex.width, bgTex.height);
            var sprite = Sprite.Create(bgTex, rect, new Vector2(0.5f, 0.5f), 1);
            _overlayImage.material.mainTexture = bgTex;
            _overlayImage.sprite = sprite;
            var newColor = _overlayImage.color;
            _overlayImage.color = newColor;
            _overlayImage.canvasRenderer.SetAlpha(0.0f);

            gameObject.transform.localScale = new Vector3(1, 1, 1);
            gameObject.GetComponent<RectTransform>().sizeDelta = _canvasObject.GetComponent<RectTransform>().sizeDelta;
            gameObject.transform.SetParent(_canvasObject.transform, false);
            gameObject.transform.SetAsFirstSibling();
        }

        public void Begin(float duration, Color fadingColor)
        {
            _duration = duration;
            _fadingColor = fadingColor;
            _startTime = Time.realtimeSinceStartup;
        }

        public void End(float duration)
        {
            _duration = duration;
            _ending = true;
            _startTime = Time.realtimeSinceStartup;
        }
    }
}
