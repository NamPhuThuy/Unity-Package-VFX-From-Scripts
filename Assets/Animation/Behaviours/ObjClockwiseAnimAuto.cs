using DG.Tweening;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public class ObjClockwiseAnimAuto : ObjActiveAuto
    {
        [Header("Stats")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Vector3 startAngle = new Vector3(0, 0, -15f);
        [SerializeField] private Vector3 endAngle = new Vector3(0, 0, 15f);

        [SerializeField] private Ease ease = Ease.InOutSine;
        private Sequence _currentSeq;
        private float _rootAngle;

        protected override void Start()
        {
            base.Start();
            _rootAngle = transform.localEulerAngles.z;
        }

        #region Override Methods

        public override void Play()
        {
            // transform.localEulerAngles = startAngle;
            _currentSeq = DOTween.Sequence();
            _currentSeq.Insert(0, transform.DOLocalRotate(endAngle, duration).SetEase(ease));
            _currentSeq.Insert(duration, transform.DOLocalRotate(startAngle, duration).SetEase(ease));
            _currentSeq.SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }

        public override void Stop()
        {
            _currentSeq.Kill();
            
            var val = transform.localEulerAngles;
            val.z = _rootAngle;
            transform.localEulerAngles = val;
        }

        #endregion
    }
}