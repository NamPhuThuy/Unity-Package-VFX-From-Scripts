using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

namespace NamPhuThuy.VFX
{
    public struct VFXHandle
    {
        public VFXType type;
        public GameObject go;
        internal float dieAt;
        public bool IsValid => go != null;
        public void Stop() { if (go) go.SetActive(false); } // immediate release
    }

    [DefaultExecutionOrder(-50)]
    public class VFXManager : Singleton<VFXManager>
    {
        [SerializeField] private VFXCatalog vfxCatalog;
        
        [SerializeField] private ResourceCollectVFXDatas resourceCollectVFXDatas;
        public ResourceCollectVFXDatas ResourceCollectVFXDatas => resourceCollectVFXDatas;
        
        // type -> pooled objects
        private readonly Dictionary<VFXType, Queue<VFXBase>> _pool = new();
        private readonly Dictionary<VFXBase, VFXType> _reverse = new();
        
        
        // active instances (for timeout cleanup)
        private readonly List<VFXHandle> _active = new();

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            base.Awake();
            PreloadAll();
        }

        void Update()
        {
            /*// safety cleanup
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var h = _active[i];
                if (!h.go || !h.go.activeSelf || Time.unscaledTime >= h.dieAt)
                {
                    if (h.go) ReturnToPool(h.type, h.go);
                    _active.RemoveAt(i);
                }
            }*/
        }

        #endregion

        void PreloadAll()
        {
            if (!vfxCatalog) return;
            foreach (var e in vfxCatalog.entries)
                Preload(e.type, e.preload);
        }

        public void Preload(VFXType type, int count)
        {
            var entry = vfxCatalog.GetEntry(type);
            if (entry == null || !entry.prefab) return;

            if (!_pool.ContainsKey(type)) _pool[type] = new Queue<VFXBase>();
            var q = _pool[type];

            while (q.Count < count)
            {
                var go = Instantiate(entry.prefab, transform);
                go.gameObject.SetActive(false);
                q.Enqueue(go);
                
                _reverse[go] = type;
            }
        }

        private VFXBase Get(VFXType type)
        {
            if (!_pool.TryGetValue(type, out var q)) { q = new Queue<VFXBase>(); _pool[type] = q; }
            if (q.Count > 0) return q.Dequeue();

            var entry = vfxCatalog.GetEntry(type);
            if (entry == null || !entry.prefab)
            {
                DebugLogger.LogWarning($"Missing VFX prefab for {type}"); 
                return null;
            }

            var inst = Instantiate(entry.prefab, transform);
            _reverse[inst] = type;
            return inst;
        }
        
        public void Release(VFXBase vfx)
        {
            if (!vfx) return;
            if (!_reverse.TryGetValue(vfx, out var type)) return;

            vfx.StopImmediate();
            vfx.transform.SetParent(transform, false);
            vfx.gameObject.SetActive(false);
            _pool[type].Enqueue(vfx);
        }
        

        #region Public Methods

        public VFXBase Play(VFXType type, VFXArguments args)
        {
            var v = Get(type);
            if (!v) return null;

            // position/orientation if given
            if (args.worldPos != default)
            {
                v.transform.SetPositionAndRotation(args.worldPos, args.worldRot);
            }
            else if (args.target)
            {
                v.transform.position = args.target.position; 
                v.transform.rotation = args.target.rotation;
            }
            else
            {
                v.transform.localPosition = Vector3.zero;
            }

            v.Play(args);
            return v;
        }

        // convenience
        public VFXBase PlayAt(
            VFXType type = VFXType.NONE, 
            Vector3 pos = default, 
            int amount = 0, 
            string message = null, 
            float duration = 0,
            Transform initialParent = null, 
            Transform target = null,
            System.Action onArrive = null, 
            System.Action onComplete = null)
        {
            return Play(type, new VFXArguments {
                worldPos = pos,
                worldRot = Quaternion.identity,
                amount = amount,
                message = message,
                duration = duration,
                initialParent = initialParent,
                target = target,
                onArrive = onArrive,
                onComplete = onComplete
            });
        }

        #endregion
    }

    public class SpawnedRewardItemsData
    {
        public int totalItems;
        public int remaningItems;
        public float unitValue;

        public float GetValueLeft()
        {
            remaningItems--;

            return remaningItems * unitValue;
        }
    }
    
    [CustomEditor(typeof(VFXManager))]
    public class VFXManagerEditor : Editor
    {
        private VFXManager _script;
        private Texture2D frogIcon;
        
        
        private VFXType selectedVFXType = VFXType.NONE;
        private Vector3 testPosition = Vector3.zero;
        private int testAmount = 100;
        private string testMessage = "Test Message";
        private float testDuration = 2f;

        private bool isUseVFXManagerPos = false;
        private void OnEnable()
        {
            _script = (VFXManager)target;
            frogIcon = Resources.Load<Texture2D>("frog");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying) return;

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

            // Quick test all VFX types
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Test All VFX Types"))
            {
                TestAllVFXTypes(_script);
            }
        }

        private void ButtonPlayCurrentVFX()
        {
            Vector3 posi = testPosition;
            if (isUseVFXManagerPos) posi = _script.transform.position;
            
            if (GUILayout.Button(new GUIContent("Play VFX", frogIcon)))
            {
                _script.PlayAt(
                    type: selectedVFXType,
                    pos: posi,
                    amount: testAmount,
                    message: testMessage,
                    duration: testDuration
                );
            }
        }

        private void ButtonPlayAtCenterSceneView()
        {
            if (GUILayout.Button(new GUIContent("Play at Scene View Center", frogIcon)))
            {
                var sceneView = SceneView.lastActiveSceneView;
                if (sceneView != null)
                {
                    Vector3 centerPos = sceneView.camera.transform.position + sceneView.camera.transform.forward * 5f;
                    _script.PlayAt(
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

/*
// 1) Coins fly to panel; update the counter when they ARRIVE:
var coinPanel = GUIManager.Ins.GUIShop.CoinPanel; // your panel Transform
var ticker = coinPanel.GetComponentInChildren<NumberTicker>();

int delta = 250;
VFXManager.Ins.PlayAt(
    VFXType.COIN_FLY,
    pos: someWorldPoint,
    amount: delta,
    target: coinPanel.transform,
    onArrive: () => ticker?.AnimateDelta(delta)
);

// 2) Simple popup text in world:
VFXManager.Ins.PlayAt(
    VFXType.POPUP_TEXT,
    pos: worldPos,
    message: "+3 Moves"
);

// 3) Just a particle burst:
VFXManager.Ins.PlayAt(VFXType.HIT_SPARK, pos: hitPoint);
 */