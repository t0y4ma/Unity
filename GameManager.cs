using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("CONST Variables")]
    public float STAGE_WIDTH = 47.5f;
    public float STAGE_HEIGHT = 60.0f;
    public int MAXSPLITCOUNT = 3;
    public float ROTATE_SPEED = 2.5f;
    public float DROP_FIRSTSPEED = 25.0f;

    [Header("Public Variables")]
    public bool isCleared = false;
    public float timeSinceLastDrop = 0f;
    public bool hasAdaptiveColor = true;

    [Header("References")]
    public static GameManager instance;
    public Predictor predictor;
    public ObjManager objManager;
    public InputManager inputManager;
    public SceneLoadManager sceneManager;

    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartGame();
    }
    
    private void FixedUpdate()
    {
        if (isCleared)
        {
            
        }
        else
        {
            if (!objManager.isObjMoving || timeSinceLastDrop > 3.0f)
            {
                objManager.NextObj();
                objManager.isObjMoving = true;
            }
            timeSinceLastDrop += Time.fixedDeltaTime;
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartGame();
    }

    public void StartGame()
    {
        objManager = GetComponent<ObjManager>();
        inputManager = GetComponent<InputManager>();
        sceneManager = GetComponent<SceneLoadManager>();
        predictor = GetComponent<Predictor>();

        objManager.StartGame();
        inputManager.StartGame();
        sceneManager.StartGame();
        isCleared = false;
        Debug.Log("GameManager StartGame");
    }

    public void ClearGame()
    {
        isCleared = true;
        Debug.Log("Clear!");
        sceneManager.gameText.SetText("Game Clear!");
        sceneManager.restartButton.gameObject.SetActive(true);
        sceneManager.restartButton.enabled = true;
    }
}
