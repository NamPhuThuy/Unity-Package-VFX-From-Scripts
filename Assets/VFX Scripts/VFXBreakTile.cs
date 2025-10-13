/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

namespace NamPhuThuy.AnimateWithScripts
{
    public class VFXBreakTile : VFXBase
    {
        [SerializeField] private SkeletonAnimation skeletonAnimation;

        [SpineAnimation] public string idleAnimation;
        [SpineAnimation] public string[] breakTileAnimations;

        protected override void OnPlay()
        {
            skeletonAnimation.gameObject.SetActive(true);

            skeletonAnimation.AnimationState.SetAnimation(0, breakTileAnimations[Random.Range(0, breakTileAnimations.Length)], false);

            skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
        }

        private void OnAnimationComplete(TrackEntry trackEntry)
        {
            skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;

            skeletonAnimation.skeleton.SetToSetupPose();
            skeletonAnimation.AnimationState.ClearTrack(0);

            skeletonAnimation.gameObject.SetActive(false);
        }
    }
}
*/
