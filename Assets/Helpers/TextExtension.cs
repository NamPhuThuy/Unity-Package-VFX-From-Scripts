/*
Github: https://github.com/NamPhuThuy
*/

using TMPro;
using UnityEngine;

namespace NamPhuThuy.VFX
{
    
    public static class TextExtension
    {
        /// <summary>
        /// Copy visual and layout properties from one TextMeshPro (source) to another (target).
        /// </summary>
        public static void CopyProperties(this TextMeshProUGUI target, TextMeshProUGUI source)
        {
            if (target == null || source == null)
            {
                Debug.LogWarning("CopyProperties failed: source or target TextMeshPro is null.");
                return;
            }

            // Basic content
            target.text = source.text;
            target.richText = source.richText;

            // Font and material
            target.font = source.font;
            target.fontMaterial = source.fontMaterial;
            target.fontSharedMaterial = source.fontSharedMaterial;
            target.fontStyle = source.fontStyle;

            // Font sizing and spacing
            target.fontSize = source.fontSize;
            target.enableAutoSizing = source.enableAutoSizing;
            target.fontSizeMin = source.fontSizeMin;
            target.fontSizeMax = source.fontSizeMax;
            target.characterSpacing = source.characterSpacing;
            target.wordSpacing = source.wordSpacing;
            target.lineSpacing = source.lineSpacing;
            target.paragraphSpacing = source.paragraphSpacing;

            // Alignment and wrapping
            target.alignment = source.alignment;
            // target.enableWordWrapping = source.enableWordWrapping;
            target.overflowMode = source.overflowMode;
            target.horizontalAlignment = source.horizontalAlignment;
            target.verticalAlignment = source.verticalAlignment;

            // Colors and gradients
            target.color = source.color;
            target.enableVertexGradient = source.enableVertexGradient;
            target.colorGradient = source.colorGradient;
            target.colorGradientPreset = source.colorGradientPreset;
            target.faceColor = source.faceColor;
            target.outlineColor = source.outlineColor;
            target.outlineWidth = source.outlineWidth;
            target.fontMaterial = source.fontMaterial;

            // Margins and geometry
            target.margin = source.margin;
            target.extraPadding = source.extraPadding;
            target.isOrthographic = source.isOrthographic;
            target.fontFeatures = source.fontFeatures;

            // Effects
            target.enableCulling = source.enableCulling;
            // target.ignoreRectMaskCulling = source.ignoreRectMaskCulling;
            target.ignoreVisibility = source.ignoreVisibility;
            target.maskable = source.maskable;
            target.isOverlay = source.isOverlay;

            // Auto layout (optional)
            target.enableAutoSizing = source.enableAutoSizing;
            target.wordWrappingRatios = source.wordWrappingRatios;

            // Update the text mesh after applying changes
            target.ForceMeshUpdate();
        }
    }
}