using System.Runtime.CompilerServices;
using UnityEngine;

namespace A320VAU.Common
{
    public static class VLogger
    {
        public static void Info(object message, [CallerMemberName] string module = "") => 
            Debug.Log(buildLogString(message, module));
        
        public static void Info(object message, UnityEngine.Object context, [CallerMemberName] string module = "") => 
            Debug.Log(buildLogString(message, module), context);

        public static void Warn(object message, [CallerMemberName] string module = "") =>
            Debug.LogWarning(buildLogString(message, module));
        
        public static void Warn(object message, UnityEngine.Object context, [CallerMemberName] string module = "") =>
            Debug.LogWarning(buildLogString(message, module), context);
        
        public static void Error(object message, [CallerMemberName] string module = "") =>
            Debug.LogError(buildLogString(message, module));
        
        public static void Error(object message, UnityEngine.Object context, [CallerMemberName] string module = "") =>
            Debug.LogError(buildLogString(message, module), context);

        private static string buildLogString(object message, string module = "")
        {
            return module == "" ? $"[V320]{message}" : $"[V320][{module}]{message}";
        }
    }
}