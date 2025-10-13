/*using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using NamPhuThuy.VFX;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFXFromScripts
{
    
    public class VFXConfetti : VFXBase
    {
        #region Private Serializable Fields
        
        [SerializeField] private ParticleSystem confettiParticleSystem;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void OnEnable()
        {
            
        }

        #endregion

        #region Private Methods
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
            confettiParticleSystem.Play();
            transform.parent = args.initialParent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(VFXConfetti))]
    [CanEditMultipleObjects]
    public class VFXConfettiEditor : Editor
    {
        private VFXConfetti script;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (VFXConfetti)target;

            ButtonResetValues();
        }

        private void ButtonResetValues()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset Values", GUILayout.Width(InspectorConst.BUTTON_WIDTH_MEDIUM)))
            {
                script.ResetValues();
                EditorUtility.SetDirty(script); // Mark the object as dirty
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    #endif
}*/