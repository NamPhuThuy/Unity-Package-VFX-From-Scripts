using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    public class VFXBreakTileParticle : VFXBase
    {
        [SerializeField] private ParticleSystem effectParticle;

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            args.onBegin += SetValues;
        }

        #endregion
        
        private void SetValues()
        {
            transform.parent = args.initialParent;
        }
        
        protected override void OnPlay()
        {
            // Debug.Log($"VFXBreakTileParticle.OnPlay()");
            
            // transform.localPosition = args.worldPos;

            // Vector3 currentEuler = transform.localRotation.eulerAngles;
            // transform.localRotation = Quaternion.Euler(-65f, 30f, -121f);
            transform.localPosition = new Vector3(0, 0.5f, -1);
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            // Debug.Log($"VFXBreakTileParticle.OnPlay() - position: {transform.position}");
            effectParticle.Play();
        }
    }
}