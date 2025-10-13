/*
Github: https://github.com/NamPhuThuy
*/

using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace NamPhuThuy.VFXFromScripts
{
    
    public class ColorHelper
    {
        public static Color GetRandomPastelColor()
        {
            // Generate random hue (0-1)
            float hue = Random.Range(0f, 1f);
    
            // Use high saturation and value for pastel effect
            float saturation = Random.Range(0.3f, 0.7f); // Medium saturation for soft colors
            float value = Random.Range(0.8f, 1f);        // High brightness for light colors
    
            return Color.HSVToRGB(hue, saturation, value);
        }

        public static Color GetRandomPastelColor(float alpha)
        {
            Color pastelColor = GetRandomPastelColor();
            pastelColor.a = alpha;
            return pastelColor;
        }

        public static Color GetRandomPastelColor(float minSaturation = 0.3f, float maxSaturation = 0.7f, 
            float minValue = 0.8f, float maxValue = 1f)
        {
            float hue = Random.Range(0f, 1f);
            float saturation = Random.Range(minSaturation, maxSaturation);
            float value = Random.Range(minValue, maxValue);
    
            return Color.HSVToRGB(hue, saturation, value);
        }
        
        public static Color GetTextColorFromBackground(Color backgroundColor)
        {
            Color.RGBToHSV(backgroundColor, out float h, out float s, out float v);

            Color randomTextColor = Color.white;

            float luminance = CalculateLuminance(randomTextColor);
            if (luminance < 0.5f)
            {
                randomTextColor = AdjustColorLightness(randomTextColor, 0.8f);
            }
            else
            {
                randomTextColor = AdjustColorLightness(randomTextColor, 0.2f);
            }

            return randomTextColor;
        }

        public static KeyValuePair<Color, Color> RandomContrastColorPair()
        {
            var firstColor = new Color(Random.value, Random.value, Random.value);
            
            var secondColor = Color.white;

            Color.RGBToHSV(firstColor, out float h, out float s, out float v);

            float luminance = CalculateLuminance(firstColor);
            /*if (luminance < 0.5f)
            {
                secondColor = AdjustColorLightness(secondColor, 0.8f);
            }
            else
            {
                secondColor = AdjustColorLightness(secondColor, 0.2f);
            }*/
            
            if (luminance < 0.4f)
            {
                // Dark background - use bright/light colors
                secondColor = GetRandomBrightColor();
            }
            else if (luminance > 0.6f)
            {
                // Light background - use dark/saturated colors
                secondColor = GetRandomDarkColor();
            }
            else
            {
                // Medium background - use high contrast colors
                secondColor = Random.value > 0.5f ? GetRandomBrightColor() : GetRandomDarkColor();
            }
            
            return new KeyValuePair<Color, Color>(firstColor, secondColor);
        }
        
        private static Color AdjustColorLightness(Color color, float factor)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            v = Mathf.Clamp01(factor);
            return Color.HSVToRGB(h, s, v);
        }
        
        /// <summary>
        /// Calculate luminance of a color based on its RGB components.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static float CalculateLuminance(Color color)
        {
            float r = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
            float g = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
            float b = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }

        #region Generators

        private static Color GetRandomBrightColor()
        {
            float hue = Random.Range(0f, 1f);
            float saturation = Random.Range(0.6f, 1f);  // High saturation for vivid colors
            float value = Random.Range(0.8f, 1f);       // High brightness
            return Color.HSVToRGB(hue, saturation, value);
        }
    
        private static Color GetRandomDarkColor()
        {
            float hue = Random.Range(0f, 1f);
            float saturation = Random.Range(0.7f, 1f);  // High saturation
            float value = Random.Range(0.2f, 0.5f);     // Low brightness for dark colors
            return Color.HSVToRGB(hue, saturation, value);
        }

        #endregion
      
    }
}