using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


namespace NamPhuThuy.AnimateWithScripts
{
    public class ObjScaleAuto : ObjActiveAuto
    {
        [Header("Stats")] 
        [SerializeField] private Vector3 originalScale = Vector3.one;
        [SerializeField] private Vector3 targetScale = Vector3.one * 0.9f;
        
        [SerializeField] private float scaleMultiplier = 0.9f;
        
        [SerializeField] private float time = 0.5f;
        [SerializeField] private float time2 = 0.5f;

        [Header("Behavior")] 
        private Sequence currentSequence;

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        
        #region MonoBehaviour

        public override void Play()
        {
            currentSequence = DOTween.Sequence();
            currentSequence.Insert(0, transform.DOScale(targetScale, time).SetEase(Ease.InOutSine));
            currentSequence.Insert(time, transform.DOScale(originalScale, time2).SetEase(Ease.InOutSine));
            currentSequence.SetLoops(-1).SetUpdate(true);
        }

        public override void Stop()
        {
            currentSequence?.Kill();
            transform.localScale = originalScale;
        }

        private void OnValidate()
        {
            originalScale = transform.localScale;
            targetScale = originalScale * scaleMultiplier;
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion
    }
}