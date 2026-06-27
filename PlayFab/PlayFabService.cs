using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabService : IPlayFabService
{
    private bool isLoggedIn = false;
    private const string DEVICE_ID_KEY = "PlayFab_DeviceID";
    private const string LOGIN_CACHE_KEY = "PlayFab_LastLogin";
    private const string USER_ID_CACHE_KEY = "PlayFab_UserID";

    private string GetDeviceId()
    {
#if UNITY_WEBGL
        if (!PlayerPrefs.HasKey(DEVICE_ID_KEY))
        {
            string deviceId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(DEVICE_ID_KEY, deviceId);
            PlayerPrefs.Save();
            PlayFabDebug.Log($"New WebGL Device ID created: {deviceId}");
        }
        string webglDeviceId = PlayerPrefs.GetString(DEVICE_ID_KEY);
        PlayFabDebug.Log($"Using WebGL Device ID: {webglDeviceId}");
        return webglDeviceId;
#else
        return SystemInfo.deviceUniqueIdentifier;
#endif
    }

    public void Login(Action onSuccess, Action<string> onError)
    {
        ValidateWebGLEnvironment();

        var request = new LoginWithCustomIDRequest
        {
            CustomId = GetDeviceId(),
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                isLoggedIn = true;
                CacheLoginInfo(result.PlayFabId);
                PlayFabDebug.Log("Login successful");
                onSuccess?.Invoke();
            },
            error =>
            {
                PlayFabDebug.LogError($"Login failed: {error.GenerateErrorReport()}");
                onError?.Invoke(error.GenerateErrorReport());
            });
    }

    private void CacheLoginInfo(string playFabId)
    {
#if UNITY_WEBGL
        PlayerPrefs.SetString(LOGIN_CACHE_KEY, System.DateTime.Now.ToString());
        PlayerPrefs.SetString(USER_ID_CACHE_KEY, playFabId);
        PlayerPrefs.Save();
#endif
    }

    public bool TryRestoreSession()
    {
#if UNITY_WEBGL
        if (PlayerPrefs.HasKey(USER_ID_CACHE_KEY))
        {
            isLoggedIn = true;
            PlayFabDebug.Log("Session restored from cache");
            return true;
        }
#endif
        return false;
    }

#if UNITY_WEBGL
    private void ValidateWebGLEnvironment()
    {
        string url = Application.absoluteURL;
        bool isHttps = url.StartsWith("https://") || url.Contains("localhost") || url.Contains("127.0.0.1");
        
        if (!isHttps && !string.IsNullOrEmpty(url))
        {
            PlayFabDebug.LogError("WebGL environment detected but HTTPS is not used. Some features may not work.");
        }
    }
#else
    private void ValidateWebGLEnvironment() { }
#endif

    public void SendScore(int score, Action onSuccess, Action<string> onError)
    {
        if (!isLoggedIn)
        {
            PlayFabDebug.LogError("SendScore: Not logged in");
            onError?.Invoke("Not logged in");
            return;
        }

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "HighScore",
                    Value = score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result =>
            {
                PlayFabDebug.Log($"Score sent successfully: {score}");
                onSuccess?.Invoke();
            },
            error =>
            {
                PlayFabDebug.LogError($"SendScore failed: {error.GenerateErrorReport()}");
                onError?.Invoke(error.GenerateErrorReport());
            });
    }

    public void GetLeaderboard(Action<List<(string name, int score)>> onSuccess, Action<string> onError)
    {
        if (!isLoggedIn)
        {
            PlayFabDebug.LogError("GetLeaderboard: Not logged in");
            onError?.Invoke("Not logged in");
            return;
        }

        var request = new GetLeaderboardRequest
        {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(request,
            result =>
            {
                var list = new List<(string, int)>();

                foreach (var item in result.Leaderboard)
                {
                    list.Add((item.DisplayName, item.StatValue));
                }

                PlayFabDebug.Log($"Leaderboard retrieved: {list.Count} entries");
                onSuccess?.Invoke(list);
            },
            error =>
            {
                PlayFabDebug.LogError($"GetLeaderboard failed: {error.GenerateErrorReport()}");
                onError?.Invoke(error.GenerateErrorReport());
            });
    }
}