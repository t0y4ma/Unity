using System.Numerics;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using NUnit.Framework.Interfaces;

#region Structs

[Serializable]
public class RankingEntry
{
    public string playerName;
    public int moveCount;
    public string score;

    public string timeStamp;

    public RankingEntry(string playerName, int moveCount, BigInteger score, string timeStamp = "tmp")
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

    public static bool operator >(RankingEntry a, RankingEntry b)
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

    public static bool operator <(RankingEntry a, RankingEntry b)
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

    internal int CompareTo(RankingEntry a)
    {
        return this > a ? 1 : (this < a ? -1 : 0);
    }
}

[Serializable]
public class RankingList : IEnumerable<RankingEntry>, IEnumerable
{
    public List<RankingEntry> rankings;

    public RankingList(List<RankingEntry> rankings)
    {
        this.rankings = rankings;
    }

    public RankingEntry this[int index]
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

    internal void Add(RankingEntry rankingEntry)
    {
        rankings.Add(rankingEntry);
    }

    internal void Sort(Comparison<RankingEntry> comparison)
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

    public IEnumerator<RankingEntry> GetEnumerator()
    {
        return rankings.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

#endregion

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private string pathToSaveFile = "Rankings.json";

    public void AddScore(int comboCount)
    {
        Debug.Log("Score Added: " + BigInteger.Pow(2, comboCount));
        GameManager.instance.score += BigInteger.Pow(2, comboCount);
    }

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
        var rankings = JsonUtility.FromJson<RankingList>(json);
        if(rankings == null)
        {
            rankings = new RankingList(new List<RankingEntry>());
        }
        var newEntry = new RankingEntry("Player", GameManager.instance.movecount, GameManager.instance.score);
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

    public List<RankingEntry> LoadRankings()
    {
        string filePath = GetSaveFilePath();
        if(File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var rankings = JsonUtility.FromJson<List<RankingEntry>>(json);
            return rankings ?? new List<RankingEntry>();
        }
        else
        {
            return new List<RankingEntry>();
        }
    }
}
