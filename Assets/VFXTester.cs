/*
Github: https://github.com/NamPhuThuy
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    
    public class VFXTester : MonoBehaviour
    {
        #region Private Serializable Fields
        
        [Header("RESOURCES FLY")]
        public TextMeshProUGUI coinText;
        public Image coinImage;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            
        }

        void Update()
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

            // Test buttons
            EditorGUILayout.BeginHorizontal();
            
            ButtonPlayCurrentVFX();
            ButtonPlayAtCenterSceneView();
            

            EditorGUILayout.EndHorizontal();
            
            ButtonPlayPopupText();
            ButtonPlayCoinFly();

            // Quick test all VFX types
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Test All VFX Types"))
            {
                TestAllVFXTypes(VFXManager.Ins);
            }
        }

        #region VFX-Buttons

        private void ButtonPlayCurrentVFX()
        {
            Vector3 posi = testPosition;
            if (isUseVFXManagerPos) posi = _script.GetComponent<RectTransform>().anchoredPosition;
            
            if (GUILayout.Button(new GUIContent("Play VFX", frogIcon)))
            {
                VFXManager.Ins.PlayAt(
                    type: selectedVFXType,
                    pos: posi,
                    amount: testAmount,
                    prevAmount: 0,
                    message: testMessage,
                    duration: testDuration
                );
            }
        }

        private void ButtonPlayPopupText()
        {
            if (GUILayout.Button(new GUIContent("Play VFX Popup Text", frogIcon)))
            {
                var popupArgs = new PopupTextArgs {
                    message = "Hello!",
                    worldPos = managerAnchoredPos,
                    color = Color.white,
                    duration = 1f,
                    onComplete = null
                };
                VFXManager.Ins.Play<PopupTextArgs>(popupArgs);
            }
        }

        private void ButtonPlayCoinFly()
        {
            if (GUILayout.Button(new GUIContent("Play VFX Coin Fly", frogIcon)))
            {
                var coinPanel = _script.coinImage.transform;
                var coinText = _script.coinText.transform;
                
                var itemFlyArgs = new ItemFlyArgs {
                    amount = testAmount,
                    prevAmount = 0,
                    target = coinText.transform,
                    startPosition = VFXManager.Ins.transform.position,
                    targetInteractTransform = coinPanel.transform, // For positioning the target
                    onArrive = () => Debug.Log("Coins arrived!"),
                    onComplete = () => Debug.Log("Animation complete!")
                };
                
                VFXManager.Ins.Play<ItemFlyArgs>(itemFlyArgs);
            }
        }

        #endregion
        

        private void ButtonPlayAtCenterSceneView()
        {
            if (GUILayout.Button(new GUIContent("Play at Scene View Center", frogIcon)))
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    Vector3 centerPos = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
                    VFXManager.Ins.PlayAt(
                        type: selectedVFXType,
                        pos: centerPos,
                        amount: testAmount,
                        message: testMessage,
                        duration: testDuration
                    );
                }
            }
        }

        private void TestAllVFXTypes(VFXManager vfxManager)
        {
            var vfxTypes = System.Enum.GetValues(typeof(VFXType));
            float spacing = 2f;
            int index = 0;

            foreach (VFXType vfxType in vfxTypes)
            {
                if (vfxType == VFXType.NONE) continue;

                Vector3 pos = testPosition + Vector3.right * (index * spacing);
                vfxManager.PlayAt(
                    type: vfxType,
                    pos: pos,
                    amount: testAmount,
                    message: $"{vfxType}",
                    duration: testDuration
                );
                index++;
            }
        }
    }
}