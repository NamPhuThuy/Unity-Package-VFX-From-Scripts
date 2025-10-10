/*
Github: https://github.com/NamPhuThuy
*/

using System.Collections.Generic;
using UnityEngine;

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
            
            DebugLogger.LogSimple(message:$"anchored posi: {GetComponent<RectTransform>().anchoredPosition}");
            DebugLogger.LogSimple(message:$"rect position: {GetComponent<RectTransform>().position}");
            DebugLogger.LogSimple(message:$"transform position: {transform.position}");
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

        #region Private Methods
        
        private void PositionVFX<T>(VFXBase vfx, T args) where T : struct, IVFXArguments
        {
            // Use pattern matching or switch on Type for positioning logic
            switch (args.Type)
            {
                case VFXType.ITEM_FLY when args is ItemFlyArgs coinArgs:
                    vfx.transform.position = coinArgs.startPosition;
                    break;
                case VFXType.POPUP_TEXT when args is PopupTextArgs popupArgs:
                    vfx.transform.position = popupArgs.worldPos;
                    break;
                // ... other cases
            }
        }
        

        #endregion
        

        #region Public Methods
        
        public T Play<T>(T args) where T : struct, IVFXArguments
        {
            var vfx = Get(args.Type);
            if (!vfx) return args;

            // Position the VFX based on argument type
            PositionVFX(vfx, args);
        
            // Play with type-safe arguments
            vfx.Play(args);
            return args;
        }

        #endregion
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