using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] public Predictor predictor;
    public float STAGE_WIDTH = 47.5f;
    public float STAGE_HEIGHT = 60.0f;
    public int MAXSPLITCOUNT = 3;
    public bool isCleared = false;
    public TextMeshProUGUI gameText;
    public TextMeshProUGUI objStateText;
    public ObjManager objManager;
    public InputManager inputManager;

    public float timeSinceLastDrop = 0f;
    
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
    void Start()
    {
        gameText.SetText("");
    }
    
    void FixedUpdate()
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

    public void ClearGame()
    {
        isCleared = true;
        Debug.Log("Clear!");
        gameText.SetText("Game Clear!");
    }
}
