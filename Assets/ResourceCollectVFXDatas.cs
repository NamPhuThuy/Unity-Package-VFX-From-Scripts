using System;
using System.Collections.Generic;
using NamPhuThuy.Data;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.VFX
{
    [CreateAssetMenu(fileName = "ResourceCollectVFXDatas", menuName = "VFX/ResourceCollectVFXDatas")]
    public class ResourceCollectVFXDatas : ScriptableObject
    {
        public List<ResourceCollectVFXData> vfxDatas;
        private Dictionary<ResourceType, ResourceCollectVFXData> _dictResourceVFXs;

        #region Private Methods

        private void EnsureIndex()
        {
            if (_dictResourceVFXs != null) return;
            _dictResourceVFXs = new Dictionary<ResourceType, ResourceCollectVFXData>(vfxDatas?.Count ?? 0);
            if (_dictResourceVFXs == null) return;
            foreach (var r in vfxDatas)
            {
                if (r == null) continue;
                _dictResourceVFXs[r.resourceType] = r; // last one wins if duplicates
            }
        }

        #endregion

        #region Public Methods

        public ResourceCollectVFXData GetVFXData(ResourceType type)
        {
            EnsureIndex();
            if (_dictResourceVFXs == null) return null;
            return _dictResourceVFXs.GetValueOrDefault(type);
        }

        #endregion
    }
    
    [Serializable]
    public class ResourceCollectVFXData
    {
        public ResourceType resourceType;
        public Sprite resourceSprite;
    }
    
}