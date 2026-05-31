using System;
using System.Collections.Generic;

public interface IPlayFabService
{
    void Login(Action onSuccess, Action<string> onError);
    void SendScore(int score, Action onSuccess, Action<string> onError);
    void GetLeaderboard(Action<List<(string name, int score)>> onSuccess, Action<string> onError);
}