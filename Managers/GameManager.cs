using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vector2 = UnityEngine.Vector2;

public class GameManager : MonoBehaviour
{
    #region Const variables
    [Header("CONST Variables")]
    public float STAGE_WIDTH = 47.5f;
    public float STAGE_HEIGHT = 100.0f;
    public int MAX_SPLIT_COUNT = 3;
    public int MAX_TYPE_COUNT = 4;
    public int MAX_COLOR_COUNT = 4;
    public float ROTATE_SPEED = 2.5f;
    public float DROP_FIRSTSPEED = 25.0f;
    public int MAX_OBJECT_COUNT = 100;
    public int COUNT_UNTIL_BOMB_EXPLODE = 3;
    #endregion

    #region Game State Variables
    [Header("Public Variables")]
    public bool isCleared = false;
    public float timeSinceLastDrop = 0f;
    public float timeSinceLastSplit = 0f;
    public bool hasAdaptiveColor = true;
    public bool hasRelativeSplitCount = true;
    public bool isOnlyAnimals = false;

    public int typeCount = 4;
    public int colorCount = 4;

    public int comboCount = 0;
    public int movecount = 0;
    public BigInteger score = 0;
    public float time = 0f;
    #endregion

    #region References
    [Header("References")]
    public static GameManager instance;
    public ObjManager objManager;
    public InputManager inputManager;
    public SceneLoadManager sceneManager;
    public UIManager uiManager;
    public ScoreManager scoreManager;
    public Predictor predictor;
    #endregion
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayFabBootstrap.Service.Login(
            () => Debug.Log("Login OK"),
            err => Debug.LogError(err)
        );
    }
    
    private void FixedUpdate()
    {
        if(sceneManager.activeSceneName == "Title")
        {
            
        }
        if(sceneManager.activeSceneName == "Game"){
            if (isCleared)
            {
                comboCount = 0;
                timeSinceLastSplit = 0f;
                timeSinceLastDrop = 0f;
            }
            else
            {
                time += Time.fixedDeltaTime;
                timeSinceLastDrop += Time.fixedDeltaTime;
                timeSinceLastSplit += Time.fixedDeltaTime;
                if (!objManager.isObjMoving || timeSinceLastDrop > 3.0f)
                {
                    objManager.NextObj();
                }

                if(timeSinceLastSplit > 3.0f)
                {
                    comboCount = 0;
                }
                else
                {
                    //Debug.Log("ComboCount: "+comboCount+" TimeSinceLastSplit: "+timeSinceLastSplit);
                    uiManager.SetUIText("ComboCount", comboCount.ToString());
                    uiManager.SetImageFillAmount("ComboBar", 1.0f - timeSinceLastSplit / 3.0f);
                }
            }
            uiManager.SetUIActive(UIType.Other, "ComboCounter", comboCount > 0);
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        objManager = GetComponent<ObjManager>();
        inputManager = GetComponent<InputManager>();
        sceneManager = GetComponent<SceneLoadManager>();
        predictor = GetComponent<Predictor>();
        uiManager = GetComponent<UIManager>();

        sceneManager.OnSceneLoaded(scene, mode);
        uiManager.OnSceneLoaded(scene, mode);

        if(scene.name == "Game") StartGame();
        if(scene.name == "Title") PrepareMenu();
        if(scene.name == "Ranking") ShowRanking();
    }

    public void PrepareMenu()
    {
        inputManager.PrepareMenu();
    }

    public void StartGame()
    {
        objManager.StartGame();
        inputManager.StartGame();
        sceneManager.StartGame();
        isCleared = false;
        movecount = 0;
        score = 0;
        time = 0f;
        
        Debug.Log("GameManager StartGame");
    }

    public async Task ShowRanking()
    {
        inputManager.ShowRanking();
        await scoreManager.ShowRanking();
        Debug.Log("GameManager ShowRanking");
    }

    public void ClearGame()
    {
        isCleared = true;
        Debug.Log("Clear!");
        uiManager.SetUIText("GameText", "Game Clear!");
        //*
        bool isNewRecord = scoreManager.SaveScore();
        var rankings = scoreManager.LoadRankings();
        foreach(var ranking in rankings)
        {
            Debug.Log(ranking.playerName + ": " + ranking.score + ", " + ranking.timeStamp);
        }
        //*/
        StartCoroutine(ShowResultCoroutine(isNewRecord));
        scoreManager.SendScoreWithRetry((long)score, (long)(time*1000), movecount, "Player");
        scoreManager.FetchRanking();
    }

    private IEnumerator ShowResultCoroutine(bool isNewRecord = false)
    {
        yield return new WaitForSeconds(0.5f);
        for(int i = 20;i <= 100; i++)
        {
            yield return new WaitForSeconds(0.01f);
            uiManager.SetUIPlace(UIType.Text, "GameText", new Vector2(0, i*5-100));
        }
        
        uiManager.SetUIActive(UIType.Button, "RestartButton", true);
        uiManager.SetUIActive(UIType.Button, "TitleButton", true);
        uiManager.SetUIActive(UIType.Other, "Result", true);
        uiManager.SetUIText("MoveCount", "0");
        uiManager.SetUIText("Score", "0");
        //score = (BigInteger)BigInteger.Log10(score);
        uiManager.SetUIValueWithDelay("MoveCount", movecount);
        yield return new WaitForSeconds(0.5f);
        uiManager.SetUIValueWithDelay("Score", score);
        uiManager.SetUIText("Time", time.ToString()+"s");
        inputManager.SetButtonArea("RestartGame", inputManager.ToRect(new Vector2(-120,-420)+new Vector2(960,540), new Vector2(100, 100)));
        inputManager.SetButtonArea("TitleButton", inputManager.ToRect(new Vector2(120,-420)+new Vector2(960,540), new Vector2(100, 100)));
        if(isNewRecord)
        {
            uiManager.SetUIText("New Record", "New Record!");
            uiManager.SetUIActive(UIType.Text, "New Record", true);
        }

        yield break;
    }
}
