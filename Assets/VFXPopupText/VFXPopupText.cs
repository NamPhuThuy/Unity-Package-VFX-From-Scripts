/*
Github: https://github.com/NamPhuThuy
*/

using System;
using DG.Tweening;
using NamPhuThuy.Common;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.VFX
{
    [RequireComponent(typeof(CanvasGroup))]
    public class VFXPopupText : VFXBase
    {

        #region Component Fields

        [Header("Components")]
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        [SerializeField] TextMeshProUGUI messageText;
        public TextMeshProUGUI MessageText => messageText;
        
        [SerializeField] private Image backImage;
        #endregion

        #region Stats Fields

        private readonly float _inDuration = 0.25f;
        private readonly float _holdDuration = 0.8f;
        private readonly float _upDuration = 0.15f;      // small "wrap up" bump
        private readonly float _downFadeDuration = 0.35f;// drop down + fade out

        private readonly float _upDistance = 18f;     // quick bump up
        

        private readonly Ease _inEase = Ease.OutCubic;
        private readonly Ease _upEase = Ease.OutQuad;
        private readonly Ease _downEase = Ease.InCubic;
        
        #endregion

        [Header("Flags")]
        [SerializeField] private bool ignoreTimeScale = true; // use unscaled time

        #region Private Fields

        private Sequence _seq;
        private Vector2 _basePos;

        #endregion

        #region MonoBehaviour Callbacks

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

        #endregion
        
        protected override void OnPlay()
        {
            KillTweens();

            SetContent(args.message, args.onBegin);
            SetRandomColor();
            gameObject.SetActive(true);

            // Reset state
            _seq?.Kill(false);
            _rectTransform.anchoredPosition = _basePos;
            _rectTransform.localScale = Vector3.zero;
            _canvasGroup.alpha = 1f;

            // Build sequence
            _seq = DOTween.Sequence().SetUpdate(ignoreTimeScale);
            

            // ENTER: slide to base + fade in
            _seq.Append(_rectTransform.DOScale(1.1f, 0.7f * _inDuration).SetEase(_inEase));
            _seq.Append(_rectTransform.DOScale(1f, 0.3f * _inDuration).SetEase(_inEase));

            // HOLD
            if (_holdDuration > 0f) _seq.AppendInterval(_holdDuration);

            // WRAP UP (small bump)
            _seq.Append(_rectTransform.DOAnchorPosY(_basePos.y + _upDistance, _upDuration).SetEase(_upEase));

            // EXIT: scale down
            _seq.Append(_rectTransform.DOScale(1.1f, 0.3f * _downFadeDuration).SetEase(_downEase));
            _seq.Join(_canvasGroup.DOFade(0f, 0.7f * _downFadeDuration));
            _seq.Append(_rectTransform.DOScale(0, 0.7f * _downFadeDuration).SetEase(_downEase));

            // FINISH: deactivate
            _seq.OnComplete(() =>
            {
                _seq = null;
                gameObject.SetActive(false);
                // Reset for next play
                _rectTransform.anchoredPosition = _basePos;
                _canvasGroup.alpha = 0f;
            });
        }

        #region Private Methods

        private void SetRandomColor()
        {
            var colorPairs = ColorHelper.RandomContrastColorPair();
            DebugLogger.Log($"VFXPopupText.SetRandomColor: BG {colorPairs.Key}, Text {colorPairs.Value}");
            
            backImage.color = colorPairs.Key;
            messageText.color = colorPairs.Value;
        }

        #endregion

        #region Public Methods

        public void SetContent(string message)
        {
            messageText.text = message;
        }
        
        public void SetContent(string message, Action moreSetup)
        {
            messageText.text = message;
            moreSetup?.Invoke();
        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VFXPopupText)), CanEditMultipleObjects]
    public class VFXPopupTextEditor : Editor
    {
        [Header("Editor")]
        private VFXPopupText _script;
        private Texture2D frogIcon;

        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            _script = (VFXPopupText)target;

            ButtonSetCurrentColorPair();
        }

        private void ButtonSetCurrentColorPair()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Set Current Color", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                EditorUtility.SetDirty(_script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
#endif
    
    public class VFXPopupTextMessage
    {
        public const string BACKGROUND_UPDATED = "Background Updated";
        public const string BOOSTER_IS_LOCKED = "Booster is locked";
        public const string IMAGE_DOWNLOADED = "Image Downloaded";
        public const string NOT_ENOUGH_COINS = "Not enough coins";
        public const string CANT_DECREASE_MORE = "Can't decrease more";
        public const string CANT_INCREASE_MORE = "Can't increase more";

        // GUI Fortune Wheel
        public const string OUT_OF_FREE_SPIN = "Out of free spin";
        
        // GUI HUD
        public const string CANT_CLICK = "Can't click";

        // GALLERY
        public const string OUT_OF_PICTURES = "Out of pictures";
        public const string NOTHING_TO_SHOW = "Nothing to show";

        public const string NO_PICTURE = "No picture";
        public const string ALREADY_CLICKED_PLAY = "Already clicked play";
        public const string PLEASE_RATE_FIRST = "Please rate first";
        public const string YOURE_TAPPING_TOO_FAST = "You're tapping too fast";

        public static readonly string[] TileClearedMessages = {
            "Amazing",
            "Awesome",
            "Excellent",
            "Great",
            "Incredible",
            "Perfect",
        };
    }
}