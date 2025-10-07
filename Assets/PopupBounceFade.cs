using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace NamPhuThuy.VFX
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupBounceFade : VFXBase
    {
        [Header("Refs")]
        [SerializeField] private RectTransform root;   // The container RectTransform
        [SerializeField] private CanvasGroup cg;

        [Header("Timing")]
        [SerializeField] private float inDuration = 0.25f;
        [SerializeField] private float holdDuration = 0.8f;
        [SerializeField] private float upDuration = 0.15f;      // small "wrap up" bump
        [SerializeField] private float downFadeDuration = 0.35f;// drop down + fade out

        [Header("Motion")]
        [SerializeField] private Vector2 enterOffset = new Vector2(0f, -40f); // starts below then slides up
        [SerializeField] private float upDistance = 18f;     // quick bump up
        [SerializeField] private float downDistance = 60f;   // then drop down

        [Header("Easing")]
        [SerializeField] private Ease inEase = Ease.OutCubic;
        [SerializeField] private Ease upEase = Ease.OutQuad;
        [SerializeField] private Ease downEase = Ease.InCubic;

        [Header("Options")]
        [SerializeField] private bool ignoreTimeScale = true; // use unscaled time

        private Sequence _seq;
        private Vector2 _basePos;

        #region MonoBehaviour Callbacks

        void Awake()
        {
            if (!root) root = GetComponent<RectTransform>();
            if (!cg) cg = GetComponent<CanvasGroup>();
            _basePos = root.anchoredPosition;
            cg.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            Play();
        }

        void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }
        
        void Reset()
        {
            root = GetComponent<RectTransform>();
            cg = GetComponent<CanvasGroup>();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Plays the popup effect with optional text & icon.
        /// </summary>
        private void Play()
        {
            gameObject.SetActive(true);

            // Reset state
            _seq?.Kill(false);
            root.anchoredPosition = _basePos + enterOffset;
            cg.alpha = 0f;

            // Build sequence
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);

            // Enter: slide to base + fade in
            _seq.Append(root.DOAnchorPos(_basePos, inDuration).SetEase(inEase));
            _seq.Join(cg.DOFade(1f, inDuration));

            // Hold
            if (holdDuration > 0f) _seq.AppendInterval(holdDuration);

            // Wrap up (small bump)
            _seq.Append(root.DOAnchorPosY(_basePos.y + upDistance, upDuration).SetEase(upEase));

            // Then drop down & fade out
            _seq.Append(
                DOTween.Sequence()
                    .Join(root.DOAnchorPosY(_basePos.y - downDistance, downFadeDuration).SetEase(downEase))
                    .Join(cg.DOFade(0f, downFadeDuration))
            );

            // Finish: deactivate
            _seq.OnComplete(() =>
            {
                _seq = null;
                gameObject.SetActive(false);
                // Reset for next play
                root.anchoredPosition = _basePos;
                cg.alpha = 0f;
            });
        }

        #endregion

        protected override void OnPlay()
        {
            throw new NotImplementedException();
        }
    }
}
