/*
Github: https://github.com/NamPhuThuy
*/

using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

/*
Keep the suffix of class is "Logger" and prefix of method is "Log"
 - Reddit: https://www.reddit.com/r/Unity3D/comments/17eikh0/i_found_a_way_to_go_to_the_right_line_in_your/
 - Sample gist: https://gist.github.com/AnatoleCF/9f4b28750ebd8c30bebd3cd04f04e520
 - Any class whose name ends with "Logger" that implements a method starting with "Log" is ignored by the console's double click, unless it is the last call in the stack trace.
 - 
 */

namespace NamPhuThuy.VFXFromScripts
{
    /// <summary>
    /// 
    /// </summary>
    public static class DebugLogger
    {
        public static bool enableLog = true;

        #region Log Error

        public static void LogError(string content, Color color, bool setBold = false)
        {
            if (!enableLog)
                return;
            Debug.LogError(ColorizedText(content, color, setBold));
            return;
        }
        
        public static void LogError(string content, bool setBold = false)
        {
            if (!enableLog)
                return;
            Debug.LogError(ColorizedText(content, setBold));
            return;
        }


        #endregion

        #region Log Warning
        
        public static void LogWarning(string content, Color color, bool setBold = false)
        {
            if (!enableLog)
                return;
            Debug.LogWarning(ColorizedText(content, color, setBold));
            return;
        }
        
        public static void LogWarning(string content, bool setBold = false)
        {
            if (!enableLog)
                return;
            Debug.LogWarning(ColorizedText(content, setBold));
            return;
        }

        #endregion
        
        #region Log 

        public static void Log(string content, Color color, bool setBold = false)
        {
            if (!enableLog)
                return;
            Debug.Log(ColorizedText(content, color, setBold));
            return;
        }
        
        public static void Log(string content, bool setBold = false)
        {
            if (!enableLog)
                return;
            Debug.Log(ColorizedText(content, setBold));
            return;
        }
        
        public static void LogSimple(
            [CallerLineNumber] int line = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = "",
            string message = ""
            /*, [CallerArgumentExpression("message")] string expression = ""*/
        )
        {
            string className = Path.GetFileNameWithoutExtension(filePath);

            Log($"{className}().{memberName}() - {message}", Color.cyan);
            /*Can replace UnityEngine.Debug.Log with any logging API you want

            Usage is simple: just put a LogCaller(); at any line you want. The compiler will pass in the 3 parameters for you.*/
        }

        

        #endregion

        #region Helper

        public static string ColorizedText(string content, Color color, bool setBold = false)
        {
            if (setBold)
                return $"<b><color=#{ColorUtility.ToHtmlStringRGB(color)}>{content}</color></b>";
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{content}</color>";
        }
        
        public static string ColorizedText(string content, bool setBold = false)
        {
            if (setBold)
                return $"<b>{content}</b>";
            return $"{content}";
        }

        #endregion
    }
}
