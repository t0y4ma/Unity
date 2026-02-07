using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField]
    private InputActionProperty lButton;
    [SerializeField] public Predictor predictor;
    public float stagesize = 4.75f;
    public float stageHeight = 6.0f;
    public bool isCleared = false;
    public TextMeshProUGUI gameText;
    public TextMeshProUGUI objStateText;
    public ObjControlManager objControlManager;

    private void OnDestroy() => lButton.action.performed -= PressAction;
    private void OnEnable() => lButton.action.Enable();
    private void OnDisable() => lButton.action.Disable();
    private void Awake()
    {
        lButton.action.performed += PressAction;
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
            
        }
    }

    private void PressAction(InputAction.CallbackContext context)
    {
        if(isCleared || objControlManager.ControllingObj == null) return;
        var pointer = Pointer.current;
        if (pointer == null)
            return;

        var position = pointer.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up,new Vector3(0, stageHeight, 0));
        float dist;
        //GenerateObject(0, new Vector3(0,stageHeight,0));
        if (plane.Raycast(ray, out dist))
        {
            // ちょっと範囲外なら補正するようにする
            
            Vector3 hitPoint = ray.GetPoint(dist);
            //Debug.Log(hitPoint);
            if(Mathf.Abs(hitPoint.x) >= stagesize-1.0f || Mathf.Abs(hitPoint.z) >= stagesize-1.0f){
                Debug.Log(hitPoint);
                if(Mathf.Abs(hitPoint.x) >= stagesize-1.0f && Mathf.Abs(hitPoint.x)*0.65f <= stagesize-1.0f) hitPoint.x = (stagesize-1.1f)*Mathf.Sign(hitPoint.x);
                if(Mathf.Abs(hitPoint.z) >= stagesize-1.0f && Mathf.Abs(hitPoint.z)*0.65f <= stagesize-1.0f) hitPoint.z = (stagesize-1.1f)*Mathf.Sign(hitPoint.z);
                Debug.Log(hitPoint);
                if(Mathf.Abs(hitPoint.x) >= stagesize-1.0f || Mathf.Abs(hitPoint.z) >= stagesize-1.0f) return;
            }

            if (objControlManager.isPlayerControllingObj)
            {
                objControlManager.ControllingObj.destination = hitPoint;
                predictor.RemovePredict();
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
