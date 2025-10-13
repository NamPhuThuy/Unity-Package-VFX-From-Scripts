/*
Github: https://github.com/NamPhuThuy
*/

using UnityEngine;

namespace NamPhuThuy.VFXFromScripts
{
    public class VFXScreenShake : VFXBase
    {
        [SerializeField] private Camera targetCamera;
        
        private Vector3 originalPosition;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeIntensity;
        private AnimationCurve shakeCurve;
        
        private void Start()
        {
            if (!targetCamera)
                targetCamera = Camera.main;
        }
        
        public override void Play<T>(T args)
        {
            if (args is ScreenShakeArgs shakeArgs)
            {
                shakeIntensity = shakeArgs.intensity;
                shakeDuration = shakeArgs.duration;
                shakeCurve = shakeArgs.shakeCurve ?? AnimationCurve.EaseInOut(0, 1, 1, 0);
                
                if (targetCamera)
                {
                    originalPosition = targetCamera.transform.localPosition;
                    shakeTimer = 0f;
                    gameObject.SetActive(true);
                }
            }
        }

        public override void Play(object args)
        {
            throw new System.NotImplementedException();
        }

        public void StopImmediate()
        {
            if (targetCamera)
                targetCamera.transform.localPosition = originalPosition;
            
            gameObject.SetActive(false);
        }
        
        private void Update()
        {
            if (!targetCamera) return;
            
            if (shakeTimer < shakeDuration)
            {
                shakeTimer += Time.deltaTime;
                float normalizedTime = shakeTimer / shakeDuration;
                float curveValue = shakeCurve.Evaluate(normalizedTime);
                float currentIntensity = shakeIntensity * curveValue;
                
                Vector3 shakeOffset = Random.insideUnitSphere * currentIntensity;
                shakeOffset.z = 0; // Keep camera on same Z plane
                
                targetCamera.transform.localPosition = originalPosition + shakeOffset;
            }
            else
            {
                targetCamera.transform.localPosition = originalPosition;
                VFXManager.Ins.Release(this);
            }
        }
    }
}
