/*
Github: https://github.com/NamPhuThuy
*/

using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.VFX
{
    public interface IVFXArguments
    {
        VFXType Type { get; }
        
       
    }
    
    public struct CoinFlyArgs : IVFXArguments
    {
        public VFXType Type => VFXType.COIN_FLY;
        
        public int amount;
        public int prevAmount;
        public Transform target;
        public Transform targetInteractTransform;
        public Sprite itemSprite;
        public Vector3 startPosition;
        public System.Action onArrive;
        public System.Action onComplete;
    }
    
    public struct PopupTextArgs : IVFXArguments
    {
        public VFXType Type => VFXType.POPUP_TEXT;
        
        public string message;
        public Vector3 worldPos;
        public Color color;
        public float duration;
        public System.Action onComplete;
    }
}