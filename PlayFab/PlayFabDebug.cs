using UnityEngine;

public static class PlayFabDebug
{
    public static void Log(string message)
    {
#if UNITY_WEBGL
        Debug.Log($"[PlayFab WebGL] {message}");
#else
        Debug.Log($"[PlayFab] {message}");
#endif
    }

    public static void LogWarning(string message)
    {
#if UNITY_WEBGL
        Debug.LogWarning($"[PlayFab WebGL] {message}");
#else
        Debug.LogWarning($"[PlayFab] {message}");
#endif
    }

    public static void LogError(string message)
    {
#if UNITY_WEBGL
        Debug.LogError($"[PlayFab WebGL ERROR] {message}");
#else
        Debug.LogError($"[PlayFab ERROR] {message}");
#endif
    }
}
