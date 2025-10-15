/*
Github: https://github.com/NamPhuThuy
*/

using System;
using UnityEngine;

namespace NamPhuThuy.AnimateWithScripts
{
    public interface IVFXArguments
    {
        VFXType Type { get; }
        
       
    }
    
    public struct ItemFlyArgs : IVFXArguments
    {
        public VFXType Type => VFXType.ITEM_FLY;
        
        public int amount;
        public int prevValue;
        public int itemAmount;
        public Transform target;
        public Transform targetInteractTransform;
        public Sprite itemSprite;
        public Vector3 startPosition;
        public System.Action onItemInteract;
        public System.Action onComplete;

        /*public ItemFlyArgs()
        {
            this.amount = 0;
            this.prevValue = 0;
            this.itemAmount = 0;
            this.target = null;
            this.targetInteractTransform = null;
            this.itemSprite = null;
            this.startPosition = Vector3.zero;
            this.onItemInteract = null;
            this.onComplete = null;
        }*/
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
    
    public struct StatChangeTextArgs : IVFXArguments
    {
        public VFXType Type => VFXType.STAT_CHANGE_TEXT;
        public int amount;
        public Color color;
        public Vector2 offset;
        public Vector2 moveDistance;
        public Transform initialParent;
        public System.Action onComplete;
    }
    
    public struct ScreenShakeArgs : IVFXArguments
    {
        public VFXType Type => VFXType.SCREEN_SHAKE;
        public float intensity;
        public float duration;
        public AnimationCurve shakeCurve;
    }
    
}