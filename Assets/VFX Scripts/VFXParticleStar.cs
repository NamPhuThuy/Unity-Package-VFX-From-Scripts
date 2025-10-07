using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    
    public class VFXParticleStar : VFXBase
    {
        #region Private Serializable Fields

        [SerializeField] private ParticleSystem effParticle;
        
        #endregion

        #region Private Fields

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
            effParticle.Play();

            transform.parent = args.initialParent;
            transform.localPosition = Vector3.zero;
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(VFXParticleStar))]
    [CanEditMultipleObjects]
    public class VFXParticleStarEditor : Editor
    {
        private VFXParticleStar script;
        private Texture2D frogIcon;
        
        private void OnEnable()
        {
            frogIcon = Resources.Load<Texture2D>("frog"); // no extension needed
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            script = (VFXParticleStar)target;

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