using System;
using UnityEngine;

namespace NamPhuThuy.VFX
{
    [Serializable]
    public struct VFXArguments
    {
        // common payload
        public int amount;           
        public int prevAmount;           
        public string message;        // e.g., "+3 Moves"
        public Transform initialParent; // where to spawn (if needed)
        public Transform targetTransform;      // where to fly/attach
        public Transform interactTransform;
        public Transform[] transforms;
        public Sprite sprite;
        public Vector3 worldPos;      
        public Vector3 offset;
        public Quaternion worldRot;   // spawn rotation (optional)
        public bool isLooping;
        public float duration;
        public Color color;
        
        
        // lifecycle callbacks (all optional)
        public System.Action onBegin;     // started playing
        public System.Action onArrive;    // e.g., reached target (good time to tick UI)
        public System.Action onStepDone;
        public System.Action onComplete;  // finished/auto-released

        // helpers
        public static VFXArguments At(Vector3 pos) => new VFXArguments { worldPos = pos, worldRot = Quaternion.identity };
    }
}