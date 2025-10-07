using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    
    public abstract class VFXBase : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Base")] 
        [SerializeField] protected float showDuration = 0.25f;
        [SerializeField] protected float lifeTime = 4.0f; // safety auto-release  
        [SerializeField] protected bool autoDisable = true; // return to pool when done  

        protected readonly List<Tween> tweens = new();
        [SerializeField] protected VFXArguments args;
        [SerializeField] protected bool isPlaying;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        #endregion

        #region Private Methods
        
        protected void KillTweens()
        {
            for (int i = 0; i < tweens.Count; i++) tweens[i]?.Kill();
            tweens.Clear();
        }
        
        protected abstract void OnPlay(); // implement visuals here
        
        #endregion

        #region Public Methods

        public void Play(VFXArguments _args)
        {
            if (isPlaying) StopImmediate(); // safety
            isPlaying = true;
            args = _args;

            gameObject.SetActive(true);
            args.onBegin?.Invoke();
            
            OnPlay(); // subclass visuals

            if (lifeTime > 0f) Invoke(nameof(Complete), lifeTime); // safety timeout
        }

        public void Complete()
        {
            if (!isPlaying) return;
            isPlaying = false;

            args.onComplete?.Invoke();
            KillTweens();

            if (autoDisable) VFXManager.Ins.Release(this);
            else gameObject.SetActive(false);
        }
        
        public void Arrive()
        {
            args.onArrive?.Invoke();
        }
        
        public void StopImmediate()
        {
            CancelInvoke(nameof(Complete));
            isPlaying = false;
            KillTweens();
            gameObject.SetActive(false);
        }
        
        #endregion
    }
}