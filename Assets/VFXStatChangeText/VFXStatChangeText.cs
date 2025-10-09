using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.VFX
{
    public class VFXStatChangeText : VFXBase
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI targetText;

        [Header("Stats")]
        [SerializeField] private Vector2 moveDistance;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float textSizeMul = 1.3f;

        private Transform _initialParent;
        private StatChangeTextArgs _currentArgs;

        void Awake()
        {
            _initialParent = transform.parent;
        }

        private void SetTextColor()
        {
            if (_currentArgs.amount >= 0)
            {
                text.text = $"+{_currentArgs.amount}";
                text.color = Color.green;
            }
            else
            {
                text.text = $"{_currentArgs.amount}";
                text.color = Color.red;
            }

            if (_currentArgs.color != default)
            {
                text.color = _currentArgs.color;
            }
        }

        private void SetValues()
        {
            moveDistance = _currentArgs.moveDistance;
            targetText = _currentArgs.initialParent.GetComponent<TextMeshProUGUI>();
        }

        private Vector3 GetRandomPos(float range)
        {
            return new Vector3(
                UnityEngine.Random.Range(-range, range),
                UnityEngine.Random.Range(-range, range),
                0f
            );
        }

        protected override void OnPlay() { }

        public override void Play<T>(T args)
        {
            if (args is StatChangeTextArgs statArgs)
            {
                _currentArgs = statArgs;
                PlayStatChangeText();
            }
            else
            {
                throw new ArgumentException("Invalid argument type for VFXStatChangeText");
            }
        }

        public override void Play(object args)
        {
            throw new NotImplementedException();
        }

        private void PlayStatChangeText()
        {
            SetValues();

            var targetRect = targetText.GetComponent<RectTransform>();
            var vfxRect = GetComponent<RectTransform>();

            transform.SetParent(targetText.transform.parent, true);

            vfxRect.sizeDelta = targetRect.sizeDelta;
            vfxRect.anchorMin = targetRect.anchorMin;
            vfxRect.anchorMax = targetRect.anchorMax;
            vfxRect.pivot = targetRect.pivot;

            Vector3 moreOffset = GetRandomPos(0.4f);
            transform.position = targetText.transform.position + (Vector3)_currentArgs.offset + moreOffset;

            text.CopyProperties(targetText);
            text.fontSize *= textSizeMul;
            text.enableAutoSizing = false;

            SetTextColor();

            text.DOFade(1f, 0f);
            gameObject.SetActive(true);

            Vector3 moveTarget = transform.position + (Vector3)moveDistance;
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(moveTarget, duration));
            seq.Join(text.DOFade(0f, duration));
            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                transform.SetParent(_initialParent, true);
                _currentArgs.onComplete?.Invoke();
            });
        }
    }
}
