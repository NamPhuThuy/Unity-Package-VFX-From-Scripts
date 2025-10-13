/*
Github: https://github.com/NamPhuThuy
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFXFromScripts
{
    
    public class VFXTester : MonoBehaviour
    {
        #region Private Serializable Fields
        
        [Header("RESOURCES FLY")]
        public TextMeshProUGUI coinText;
        public Image coinImage;
        
        #endregion
    }

    [CustomEditor(typeof(VFXTester))]
    public class VFXTesterEditor : Editor
    {
        private VFXTester _script;
        private Texture2D frogIcon;
        
        
        private VFXType selectedVFXType = VFXType.NONE;
        private Vector3 testPosition = Vector3.zero;
        private int testAmount = 100;
        private string testMessage = "Test Message";
        private float testDuration = 2f;
        private Vector2 managerAnchoredPos = Vector2.zero;
        
        private bool isUseVFXManagerPos = false;
        private void OnEnable()
        {
            _script = (VFXTester)target;
            managerAnchoredPos = VFXManager.Ins.GetComponent<RectTransform>().anchoredPosition;
            frogIcon = Resources.Load<Texture2D>("frog");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            // if (!Application.isPlaying) return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("VFX Testing", EditorStyles.boldLabel);

            // VFX Type selection
            selectedVFXType = (VFXType)EditorGUILayout.EnumPopup("VFX Type", selectedVFXType);

            // Test parameters
            testPosition = EditorGUILayout.Vector3Field("Position", testPosition);
            testAmount = EditorGUILayout.IntField("Amount", testAmount);
            testMessage = EditorGUILayout.TextField("Message", testMessage);
            testDuration = EditorGUILayout.FloatField("Duration", testDuration);
            isUseVFXManagerPos = EditorGUILayout.Toggle("Use VFXManager Position", isUseVFXManagerPos);

            EditorGUILayout.Space(5);
            
            ButtonPlayPopupText();
            ButtonPlayCoinFly();
            ButtonStatChange();
            ButtonScreenShake();
            
            // Quick test all VFX types
            EditorGUILayout.Space(5);
        }

        #region VFX-Buttons

        private void ButtonPlayPopupText()
        {
            if (GUILayout.Button(new GUIContent("Play VFX Popup Text", frogIcon)))
            {
                var args = new PopupTextArgs {
                    message = "Hello!",
                    worldPos = managerAnchoredPos,
                    color = Color.white,
                    duration = 1f,
                    onComplete = null
                };
                VFXManager.Ins.Play<PopupTextArgs>(args);
            }
            
        }

        private void ButtonPlayCoinFly()
        {
            if (GUILayout.Button(new GUIContent("Play VFX Coin Fly", frogIcon)))
            {
                var coinPanel = _script.coinImage.transform;
                var coinText = _script.coinText.transform;
                
                var args = new ItemFlyArgs {
                    amount = testAmount,
                    prevAmount = 0,
                    target = coinText.transform,
                    startPosition = VFXManager.Ins.transform.position,
                    targetInteractTransform = coinPanel.transform, // For positioning the target
                    onItemInteract = () => TurnOnStatChangeVFX(coinText),
                    onComplete = () => Debug.Log("Animation complete!")
                };
                
                VFXManager.Ins.Play(args);
            }

            void TurnOnStatChangeVFX(Transform coinText)
            {
                var args = new StatChangeTextArgs {
                    amount = 10,
                    color = Color.yellow,
                    offset = Vector2.zero,
                    moveDistance = new Vector2(0f, 30f),
                    initialParent = coinText,
                    onComplete = null
                };
                
                VFXManager.Ins.Play(args);
            }
        }
        
        private void ButtonStatChange()
        {
            if (GUILayout.Button(new GUIContent("Play State Change Text", frogIcon)))
            {
                var coinPanel = _script.coinImage.transform;
                var coinText = _script.coinText.transform;
                
                var args = new StatChangeTextArgs {
                    amount = 5,
                    color = Color.yellow,
                    offset = Vector2.zero,
                    moveDistance = new Vector2(0f, 30f),
                    initialParent = coinText,
                    onComplete = null
                };
                
                VFXManager.Ins.Play(args);
            }
        }

        private void ButtonScreenShake()
        {
            if (GUILayout.Button(new GUIContent("Play Screen Shake", frogIcon)))
            {
                VFXManager.Ins.Play(new ScreenShakeArgs
                {
                    intensity = 0.5f,
                    duration = 0.3f,
                    shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0)
                });
            }
        }



        #endregion
    }
}