/*
Github: https://github.com/NamPhuThuy
*/

using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.VFXFromScripts
{
    
    public static class ImageExtension
    {
        /// <summary>
        /// Sets the width of the RectTransform to keep the aspect ratio based on the given height.
        /// </summary>
        public static void SetSizeKeepRatioY(this Image image, float newHeight)
        {
            if (image == null || image.sprite == null) return;

            float aspect = image.sprite.rect.width / image.sprite.rect.height;
            RectTransform rectTransform = image.rectTransform;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newHeight * aspect);
        }
    }
}