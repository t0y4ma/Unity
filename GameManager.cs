using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] public Predictor predictor;
    public float stagesize = 4.75f;
    public float stageHeight = 6.0f;
    public bool isCleared = false;
    public TextMeshProUGUI gameText;
    public TextMeshProUGUI objStateText;
    public ObjManager objManager;

    public InputManager inputManager;
    
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
            if (!objManager.isObjMoving)
            {
                objManager.NextObj();
                objManager.isObjMoving = true;
            }
        }
    }

    public void ClearGame()
    {
        isCleared = true;
        Debug.Log("Clear!");
        gameText.SetText("Game Clear!");
    }
}
