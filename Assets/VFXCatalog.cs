/*
Github: https://github.com/NamPhuThuy
*/


using System;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public enum VFXType
    {
        NONE = 0,
        POPUP_TEXT = 1,
        ITEM_FLY = 2,
        STAT_CHANGE_TEXT = 3,
        SCREEN_SHAKE = 4,
        CONFETTI = 7,
        BREAK_TILE_PARTICLE = 5,
        PARTICLE_STAR = 6,
    }

    [CreateAssetMenu(fileName = "VFXCatalog", menuName = "VFX/VFX Catalog")]
    public class VFXCatalog : ScriptableObject
    {
        public List<Entry> entries = new();
        
        [NonSerialized]
        private Dictionary<VFXType, Entry> _dictTypeEntry;

        public Dictionary<VFXType, Entry> DictTypeEntry
        {
            get
            {
                if (_dictTypeEntry == null)
                    EnsureDict();
                return _dictTypeEntry;
            }
        }

        #region Callbacks

#if UNITY_EDITOR
        // private void OnValidate() => InvalidateIndex();
#endif

        #endregion
        
        #region Private Methods

        private void EnsureDict()
        {
            if (_dictTypeEntry != null) return;

            _dictTypeEntry = new Dictionary<VFXType, Entry>(entries?.Count ?? 0);
            if (entries == null) return;

            // Last write wins if duplicates exist
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;
                _dictTypeEntry[e.type] = e;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Force the index to rebuild on next use.</summary>
        public void InvalidateIndex() => _dictTypeEntry = null;
        
        /// <summary>Get entry by type (null if missing).</summary>
        public Entry GetEntry(VFXType type)
        {
            EnsureDict();
            if (_dictTypeEntry == null) return null;
            _dictTypeEntry.TryGetValue(type, out var e);
            return e;
        }

        /// <summary>Try-get pattern to avoid null checks.</summary>
        public bool TryGetEntry(VFXType type, out Entry entry)
        {
            EnsureDict();
            if (_dictTypeEntry == null)
            {
                entry = null;
                return false;
            }
            return _dictTypeEntry.TryGetValue(type, out entry);
        }
        
        /// <summary>Does an entry exist for this type?</summary>
        public bool IsContains(VFXType type)
        {
            EnsureDict();
            return _dictTypeEntry != null && _dictTypeEntry.ContainsKey(type);
        }
        
        /// <summary>Get the prefab for a VFX type (null if missing).</summary>
        public VFXBase GetPrefab(VFXType type)
        {
            return GetEntry(type)?.prefab;
        }
        
        /// <summary>Get preload count (fallback if missing).</summary>
        public int GetPreload(VFXType type, int fallback = 0)
        {
            var e = GetEntry(type);
            return e != null ? Mathf.Max(0, e.preload) : Mathf.Max(0, fallback);
        }

        /// <summary>Get safety auto-release seconds (fallback if missing).</summary>
        public float GetSoftMaxAliveSeconds(VFXType type, float fallback = 10f)
        {
            var e = GetEntry(type);
            return e != null ? Mathf.Max(0f, e.softMaxAliveSeconds) : Mathf.Max(0f, fallback);
        }

        #endregion
        
        [Serializable]
        public class Entry
        {
            public VFXType type;
            public VFXBase prefab; 
            [Min(0)] public int preload = 3;
            public float softMaxAliveSeconds = 10f; // safety auto-release
        }
        
    }
}