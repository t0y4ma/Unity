using UnityEngine;

public class PlayFabBootstrap : MonoBehaviour
{
    public static IPlayFabService Service { get; private set; }

    void Awake()
    {
        if(Service != null) return;

#if UNITY_WEBGL
        ValidateWebGLEnvironment();
#endif

        Service = new PlayFabService();
    }

#if UNITY_WEBGL
    private void ValidateWebGLEnvironment()
    {
        string url = Application.absoluteURL;
        bool isHttps = url.StartsWith("https://") || url.Contains("localhost") || url.Contains("127.0.0.1");
        
        if (!isHttps && !string.IsNullOrEmpty(url))
        {
            Debug.LogWarning($"PlayFab WebGL Warning: HTTPS is recommended for production. Current URL: {url}");
        }
        else if (string.IsNullOrEmpty(url))
        {
            Debug.Log("PlayFab: Running in editor or development environment");
        }
        else
        {
            Debug.Log("PlayFab: WebGL environment validated successfully");
        }
    }
#endif
}