using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NamPhuThuy;
using NamPhuThuy.UI;
using NamPhuThuy.VFX;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    public class VFXStatChangeText : VFXBase
    {
        #region Private Serializable Fields
        
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private TextMeshProUGUI targetText;
        
        [Header("Stats")]
        [SerializeField] private float moveY = 1f;
        [SerializeField] private float duration = 1f;
        [SerializeField] private float textSizeMul = 1.3f;

        [SerializeField] private float value;
        [SerializeField] private Vector2 offSet;
        private Transform _initialParent;

        #endregion

        #region MonoBehaviour Callbacks

        void Awake()
        {
            _initialParent = transform.parent;
            
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set value and color for the text
        /// </summary>
        private void SetTextColor()
        {
            
            if (value >= 0)
            {
                text.text = $"+{value}";
                text.color = Color.green;
            }
            else
            {
                text.text = $"{value}";
                text.color = Color.red;
            }

            if (args.color != default)
            {
                text.color = args.color;
            }
        }

        private void SetValues()
        {
            value = args.amount;
            targetText = args.initialParent.GetComponent<TextMeshProUGUI>();
        }

        private Vector3 GetRandomPos(float range)
        {
            Vector3 res = new Vector3(
                UnityEngine.Random.Range(-range, range),
                UnityEngine.Random.Range(-range, range),
                0f
            );

            return res;
        }
        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion

        protected override void OnPlay()
        {
            DebugLogger.LogSimple();
            SetValues();

            
            var targetRect = targetText.GetComponent<RectTransform>();
            var vfxRect = GetComponent<RectTransform>();

            transform.SetParent(targetText.transform.parent, worldPositionStays: true);

            // Match size, anchor, and pivot
            vfxRect.sizeDelta = targetRect.sizeDelta;
            vfxRect.anchorMin = targetRect.anchorMin;
            vfxRect.anchorMax = targetRect.anchorMax;
            vfxRect.pivot = targetRect.pivot;

            Vector3 moreOffset = GetRandomPos(0.4f);
            // Set world position to match target text
            transform.position = targetText.transform.position + args.offset + moreOffset;

            text.CopyProperties(targetText);
            text.fontSize *= textSizeMul;
            text.enableAutoSizing = false;

            SetTextColor();

            // Show and animate
            text.DOFade(1f, 0f); // Ensure visible
            gameObject.SetActive(true);

            // Animate upwards in world space and fade out
            Vector3 moveTarget = transform.position + (Vector3.up * moveY);
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(moveTarget, duration));
            seq.Join(text.DOFade(0f, duration));
            seq.OnComplete(() =>
            {
                // Reset and hide
                gameObject.SetActive(false);
                transform.SetParent(_initialParent, true);
            });
        }

        #region Override Methods
        
        public override void Play<T>(T args)
        {
            throw new NotImplementedException();
        }

        public override void Play(object args)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(VFXStatChangeText))]
    [CanEditMultipleObjects]
    public class VFXStatChangeTextEditor : Editor
    {
        private VFXStatChangeText script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (VFXStatChangeText)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Reset Values", frogIcon), GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif
}