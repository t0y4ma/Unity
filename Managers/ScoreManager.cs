using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System;
using System.IO;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Vector2 = UnityEngine.Vector2;

#region Structs

[Serializable]
public class LegacyRankingEntry
{
    public string playerName;
    public int moveCount;
    public string score;

    public string timeStamp;

    public LegacyRankingEntry(string playerName, int moveCount, BigInteger score, string timeStamp = "tmp")
    {
        this.playerName = playerName;
        this.moveCount = moveCount;
        this.score = score.ToString();
        if(timeStamp == "tmp")
        {
            this.timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else this.timeStamp = timeStamp;
    }

    public static bool operator >(LegacyRankingEntry a, LegacyRankingEntry b)
    {
        if(a.moveCount != b.moveCount)
        {
            return a.moveCount < b.moveCount;
        }
        if(a.score == b.score)
        {
            if(a.timeStamp == b.timeStamp)
            {
                return a.playerName.CompareTo(b.playerName) < 0;
            }
            else
            {
                return a.timeStamp.CompareTo(b.timeStamp) < 0;
            }
        }
        return a.score.CompareTo(b.score) > 0;
    }

    public static bool operator <(LegacyRankingEntry a, LegacyRankingEntry b)
    {
        if(a.moveCount != b.moveCount)
        {
            return a.moveCount > b.moveCount;
        }
        if(a.score == b.score)
        {
            if(a.timeStamp == b.timeStamp)
            {
                return a.playerName.CompareTo(b.playerName) > 0;
            }
            else
            {
                return a.timeStamp.CompareTo(b.timeStamp) > 0;
            }
        }
        return a.score.CompareTo(b.score) < 0;
    }

    internal int CompareTo(LegacyRankingEntry a)
    {
        return this > a ? 1 : (this < a ? -1 : 0);
    }
}

[Serializable]
public class LegacyRankingList : IEnumerable<LegacyRankingEntry>, IEnumerable
{
    public List<LegacyRankingEntry> rankings;

    public LegacyRankingList(List<LegacyRankingEntry> rankings)
    {
        this.rankings = rankings;
    }

    public LegacyRankingEntry this[int index]
    {
        get
        {
            if(index < 0 || index >= rankings.Count)
            {
                throw new IndexOutOfRangeException();
            }
            return rankings[index];
        }
        set
        {
            if(index < 0 || index >= rankings.Count)
            {
                throw new IndexOutOfRangeException();
            }
            rankings[index] = value;
        }
    }

    internal void Add(LegacyRankingEntry rankingEntry)
    {
        rankings.Add(rankingEntry);
    }

    internal void Sort(Comparison<LegacyRankingEntry> comparison)
    {
        rankings.Sort(comparison);
    }

    internal int Count()
    {
        return rankings.Count;
    }

    internal void RemoveAt(int index)
    {
        rankings.RemoveAt(index);
    }

    public IEnumerator<LegacyRankingEntry> GetEnumerator()
    {
        return rankings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[Serializable]
public class RankingEntry
{
    public string playerId;
    public string name;
    public long score;
    public long time;
    public int moveCount;
    public long updatedAt;
}

[Serializable]
class RankingWrapper
{
    public List<RankingEntry> list;
}
#endregion

public class ScoreManager : MonoBehaviour
{

    #region inGameProcess
    public bool isValidScoreMode = true;
    public void FixedUpdate()
    {
        GameManager.instance.uiManager.SetUIText("ScoreDuringGame", GameManager.instance.score.ToString());
    }

    public void AddScore(int comboCount)
    {
        //Debug.Log("Score Added: " + BigInteger.Pow(2, comboCount));
        GameManager.instance.score += BigInteger.Pow(2, comboCount)*100 + 200;
    }
    
    public void StartGame()
    {
        if(GameManager.instance.typeCount == 5 && GameManager.instance.colorCount == 3 && GameManager.instance.isOnlyAnimals)
        {
            isValidScoreMode = true;
        }
        else
        {
            isValidScoreMode = false;
        }
    }
    #endregion

    #region LocalSave
    [SerializeField] private string pathToSaveFile = "Rankings.json";

    private string GetSaveFilePath()
    {
        #if UNITY_EDITOR
        return Path.Combine(Application.dataPath, pathToSaveFile);
        #else
        return Path.Combine(Application.persistentDataPath, pathToSaveFile);
        #endif
    }

    public bool SaveScore()
    {
        string filePath = GetSaveFilePath();
        if(!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "{}");
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        string json = File.ReadAllText(filePath);
        var rankings = JsonUtility.FromJson<LegacyRankingList>(json);
        if(rankings == null)
        {
            rankings = new LegacyRankingList(new List<LegacyRankingEntry>());
        }
        var newEntry = new LegacyRankingEntry("Player", GameManager.instance.movecount, GameManager.instance.score);
        bool reslut = rankings.Count() == 0 || rankings[0] < newEntry;
        rankings.Add(newEntry);
        rankings.Sort((a, b) => b.CompareTo(a));
        if(rankings.Count() > 10)
        {
            rankings.RemoveAt(rankings.Count() - 1);
        }
        //Debug.Log("Saving Score. Current Rankings: " + JsonUtility.ToJson(rankings));
        File.WriteAllText(filePath, JsonUtility.ToJson(rankings));
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif

        return reslut;
    }

    public List<LegacyRankingEntry> LoadRankings()
    {
        string filePath = GetSaveFilePath();
        if(File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var rankings = JsonUtility.FromJson<List<LegacyRankingEntry>>(json);
            return rankings ?? new List<LegacyRankingEntry>();
        }
        else
        {
            return new List<LegacyRankingEntry>();
        }
    }
    #endregion

    #region CloudScript Save
    /// <summary>
    /// スコアをPlayFabのCloudScriptに送信する（リトライ付き）
    /// score: プレイヤーのスコア
    /// time: プレイヤーのクリア時間（ミリ秒）
    /// moveCount: プレイヤーのかかった手数
    /// name: プレイヤーの名前
    /// </summary>
    /// <param name="score"></param>
    /// <param name="time"></param>
    /// <param name="moveCount"></param>
    /// <param name="name"></param>
    public void SendScoreWithRetry(long score, long time, int moveCount, string name)
    {
        if(!isValidScoreMode) return;
        StartCoroutine(SendScoreCoroutine(score, time, moveCount, name));
    }

    private IEnumerator SendScoreCoroutine(long score, long time, int moveCount, string name)
    {
        int maxRetry = 5;
        float waitTime = 0.2f;

        for (int attempt = 0; attempt < maxRetry; attempt++)
        {
            bool done = false;
            bool success = false;
            bool shouldRetry = false;

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "UpdateScoreAndRanking",
                FunctionParameter = new Dictionary<string, object>
                {
                    { "score", score },
                    { "time", time },
                    { "moveCount", moveCount },
                    { "name", name }
                },
                GeneratePlayStreamEvent = true 
            };

            PlayFabClientAPI.ExecuteCloudScript(request,
                result =>
                {
                    success = true;
                    done = true;
                    /*
                    foreach (var log in result.Logs)
                    {
                        Debug.Log($"{log.Level}: {log.Message}");
                    }
                    Debug.Log("CloudScript Result: " + result.FunctionResult);
                    //*/
                },
                error =>
                {
                    // CloudScriptのthrowはここに来る
                    string err = error.GenerateErrorReport();

                    if (err.Contains("concurrent update"))
                    {
                        shouldRetry = true;
                    }
                    else
                    {
                        Debug.LogError(err);
                    }

                    done = true;
                });

            // 完了待ち
            while (!done) yield return null;

            if (success)
            {
                Debug.Log("送信成功");
                yield break;
            }

            if (!shouldRetry)
            {
                Debug.LogError("リトライ対象外エラー");
                yield break;
            }

            // ---- バックオフ ----
            yield return new WaitForSeconds(waitTime + UnityEngine.Random.Range(0f, 0.2f));

            // 少しずつ待機時間を増やす
            waitTime *= 2f;
        }

        Debug.LogError("リトライ上限到達");
    }

    public void FetchRanking()
    {
        StartCoroutine(GetRankingCoroutine());
    }

    private IEnumerator GetRankingCoroutine()
    {
        bool done = false;
        List<RankingEntry> resultList = null;

        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTopRanking"
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            result =>
            {
                try
                {
                    // CloudScriptの返り値はJSON配列
                    string json = result.FunctionResult.ToString();

                    // JsonUtility対策（配列ラップ）
                    string wrapped = "{\"list\":" + json + "}";

                    var parsed = JsonUtility.FromJson<RankingWrapper>(wrapped);
                    resultList = parsed.list;
                }
                catch (Exception e)
                {
                    Debug.LogError("JSON parse error: " + e.Message);
                    resultList = new List<RankingEntry>();
                }

                done = true;
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                resultList = new List<RankingEntry>();
                done = true;
            });

        // 完了待ち
        while (!done) yield return null;

        // ---- 使用例 ----
        for (int i = 0; i < resultList.Count; i++)
        {
            var r = resultList[i];
            Debug.Log($"{i + 1}位 {r.name} move:{r.moveCount} score:{r.score} time:{r.time}");
        }
    }
    public async Task<List<RankingEntry>> GetRankingAsync()
    {
        Debug.Log("ScoreManager GetRankingAsync called");
        Debug.Log("IsLoggedIn: " + PlayFabClientAPI.IsClientLoggedIn());
        while(!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.Log("Waiting for login...");
            await Task.Delay(100);
        }
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "GetTopRanking"
        };

        var tcs = new TaskCompletionSource<ExecuteCloudScriptResult>();

        PlayFabClientAPI.ExecuteCloudScript(request,
            result => tcs.SetResult(result),
            error => tcs.SetException(new Exception(error.GenerateErrorReport()))
        );

        try
        {
            var result = await tcs.Task;
            Debug.Log("CloudScript Result: " + result.FunctionResult);

            string json = result.FunctionResult.ToString();
            string wrapped = "{\"list\":" + json + "}";

            var parsed = JsonUtility.FromJson<RankingWrapper>(wrapped);
            return parsed.list ?? new List<RankingEntry>();
        }
        catch (Exception e)
        {
            Debug.LogError("GetRankingAsync Error: " + e.Message);
            return new List<RankingEntry>();
        }
    }
    
    #endregion


    #region RankingDisplay
    [SerializeField] private Canvas rankingCanvas;
    private List<RankingEntry> rankings;
    public List<GameObject> rankingTextObjects = new List<GameObject>();
    public int entriesPerPage = 10;
    [SerializeField] private int currentPage = 0;
    public async Task ShowRanking()
    {
        Debug.Log("ScoreManager ShowRanking called");
        rankingCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        rankings = await GetRankingAsync();
        currentPage = 0;
        ShowRankingPage();
        Debug.Log("ScoreManager ShowRanking");
    }

    public void ShowRankingPage()
    {
        Debug.Log("ScoreManager ShowRankingPage called");
        rankingTextObjects.ForEach(obj => Destroy(obj));
        rankingTextObjects.Clear();
        if(rankingCanvas == null || rankings == null){
            if(rankingCanvas == null) Debug.LogError("Ranking Canvas not found");
            if(rankings == null) Debug.LogError("Rankings data is null");
            return;
        }
        for(int i = 0;i < entriesPerPage; i++)
        {
            TextMeshProUGUI text;
            GameObject textObj = new GameObject();
            text = textObj.AddComponent<TextMeshProUGUI>();
            textObj.transform.SetParent(rankingCanvas.transform);
            rankingTextObjects.Add(textObj);
            int rankingIndex = currentPage * entriesPerPage + i;
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 400 - i * 80);
            if(rankingIndex < rankings.Count) {
                var r = rankings[rankingIndex];
                string textContent = "";
                if(rankingIndex % 10 == 0 && rankingIndex % 100 != 10) textContent += $"{rankingIndex + 1}st ";
                else if(rankingIndex % 10 == 1 && rankingIndex % 100 != 11) textContent += $"{rankingIndex + 1}nd ";
                else if(rankingIndex % 10 == 2 && rankingIndex % 100 != 12) textContent += $"{rankingIndex + 1}rd ";
                else textContent += $"{rankingIndex + 1}th : ";
                textContent += $"{r.name} moves to clear:{r.moveCount} score:{r.score} cleartime:";
                if(r.time >= 60000) textContent += $"{TimeSpan.FromMilliseconds(r.time):mm\\:ss\\.fff}";
                else textContent += $"{TimeSpan.FromMilliseconds(r.time):ss\\.fff}";
                text.text = textContent;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                text.text = "";
            }
        }
    }

    public void NextPage()
    {
        if(currentPage == rankings.Count / entriesPerPage) return;
        currentPage++;
        ShowRankingPage();
    }
    public void PreviousPage()
    {
        if(currentPage == 0) return;
        currentPage--;
        ShowRankingPage();
    }
    #endregion
}
