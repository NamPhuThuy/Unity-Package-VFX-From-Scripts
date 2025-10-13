using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.AnimateWithScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class VFXPopupText : VFXBase
    {
        [Header("Components")]
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] TextMeshProUGUI messageText;
        public TextMeshProUGUI MessageText => messageText;
        [SerializeField] private Image backImage;

        private readonly float _inDuration = 0.25f;
        private readonly float _holdDuration = 0.8f;
        private readonly float _upDuration = 0.15f;
        private readonly float _downFadeDuration = 0.35f;
        private readonly float _upDistance = 18f;
        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true;

        private Sequence _seq;
        private Vector2 _basePos;

        void Awake()
        {
            if (!_canvasGroup) _canvasGroup = GetComponent<CanvasGroup>();
            if (!_rectTransform) _rectTransform = GetComponent<RectTransform>();
            _basePos = _rectTransform.anchoredPosition;
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }

        void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void Play<T>(T args)
        {
            if (args is PopupTextArgs popupArgs)
            {
                PlayPopupText(popupArgs);
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXPopupText");
            }
        }

        public override void Play(object args)
        {
            if (args is PopupTextArgs popupArgs)
            {
                PlayPopupText(popupArgs);
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXPopupText");
            }
        }

        private void PlayPopupText(PopupTextArgs args)
        {
            KillTweens();

            SetContent(args.message, args.onComplete);
            SetRandomColor();
            gameObject.SetActive(true);

            _seq?.Kill(false);
            _rectTransform.anchoredPosition = _basePos;
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 1f;

            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            _seq.Append(_rectTransform.DOScale(1.1f, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(_rectTransform.DOScale(1f, 0.3f * _inDuration).SetEase(_inEase));
            if (_holdDuration > 0f) _seq.AppendInterval(_holdDuration);
            _seq.Append(_rectTransform.DOAnchorPosY(_basePos.y + _upDistance, _upDuration).SetEase(_upEase));
            _seq.Append(_rectTransform.DOScale(1.1f, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _downFadeDuration).SetEase(_downEase));
            _seq.OnComplete(() =>
            {
                _seq = null;
                gameObject.SetActive(false);
                _rectTransform.anchoredPosition = _basePos;
                _canvasGroup.alpha = 0f;
                args.onComplete?.Invoke();
            });
        }

        private void SetRandomColor()
        {
            var colorPairs = ColorHelper.RandomContrastColorPair();
            backImage.color = colorPairs.Key;
            messageText.color = colorPairs.Value;
        }

        public void SetContent(string message)
        {
            messageText.text = message;
        }

        public void SetContent(string message, Action moreSetup)
        {
            messageText.text = message;
            moreSetup?.Invoke();
        }
    }
}
