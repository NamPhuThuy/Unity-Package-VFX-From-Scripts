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
        
        // Generic play method that each VFX implements
        public abstract void Play<T>(T args) where T : struct, IVFXArguments;
    
        // Or use object if you prefer runtime type checking
        public abstract void Play(object args);

        public void Complete()
        {
            if (!isPlaying) return;
            isPlaying = false;

            KillTweens();

            if (autoDisable) VFXManager.Ins.Release(this);
            else gameObject.SetActive(false);
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