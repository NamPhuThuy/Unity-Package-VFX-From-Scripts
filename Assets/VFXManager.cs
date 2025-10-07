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

        public VFXBase PlayAt(
            VFXType type = VFXType.NONE, 
            int amount = 0, 
            int prevAmount = 0,
            string message = null, 
            Transform initialParent = null, 
            Transform target = null,
            Vector3 pos = default,
            Vector3 offset = default,
            Quaternion rot = default,
            float duration = 0,
            bool isLooping = false, 
            Color color = default,
            Action onArrive = null, 
            Action onStepDone = null,
            Action onComplete = null)
        {
            return Play(type, new VFXArguments {
                amount = amount,
                prevAmount = prevAmount,
                message = message,
                initialParent = initialParent,
                target = target,
                worldPos = pos,
                offset = offset,
                worldRot = rot,
                isLooping = isLooping,
                duration = duration,
                color = color,
                onArrive = onArrive,
                onStepDone = onStepDone,
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