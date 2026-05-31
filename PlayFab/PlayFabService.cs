using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

public class PlayFabService : IPlayFabService
{
    private bool isLoggedIn = false;

    public void Login(Action onSuccess, Action<string> onError)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = UnityEngine.SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                isLoggedIn = true;
                onSuccess?.Invoke();
            },
            error =>
            {
                onError?.Invoke(error.GenerateErrorReport());
            });
    }

    public void SendScore(int score, Action onSuccess, Action<string> onError)
    {
        if (!isLoggedIn)
        {
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
            result => onSuccess?.Invoke(),
            error => onError?.Invoke(error.GenerateErrorReport()));
    }

    public void GetLeaderboard(Action<List<(string name, int score)>> onSuccess, Action<string> onError)
    {
        if (!isLoggedIn)
        {
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

                onSuccess?.Invoke(list);
            },
            error => onError?.Invoke(error.GenerateErrorReport()));
    }
}