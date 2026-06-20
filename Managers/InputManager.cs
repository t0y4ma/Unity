using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

#region Structs
enum ActionType
{
    Normal,
    Hold,
    Release
}
class ButtonArea
{
    public Rect area;
    public Action action;
    public int priority; // おおきいほどつよい

    public ActionType actionType = ActionType.Normal;
    public ButtonArea(Rect area, Action action, int priority = 0, ActionType actionType = ActionType.Normal)
    {
        this.area = area;
        this.action = action;
        this.priority = priority;
        this.actionType = actionType;
    }
    public bool Contains(Vector2 point)
    {
        return area.Contains(point);
    }

    public void setArea(Rect area)
    {
        this.area = area;
    }

    public void DoAction()
    {
        action?.Invoke();
    }
}
#endregion

public class InputManager : MonoBehaviour
{
    private Dictionary<string, ButtonArea> buttonAreas;
    
    [SerializeField] private InputActionProperty lButton;
    [SerializeField] private InputActionProperty spaceBar;
    [SerializeField] private InputActionProperty rKey;
    [SerializeField] private InputActionProperty cKey;
    [SerializeField] private InputActionProperty hKey;

    [SerializeField] private InputActionProperty[] arrowKeys = new InputActionProperty[4]; // LRUD
    public Dictionary<string, bool> props;

    public float prev_width = 1920f;
    public float prev_height = 1080f;
    public float timeSinceLastHold = 0f;

    #region Scale to Screen Size
    private Rect ScaleRect(Rect rect)
    {
        return new Rect(
            rect.x * Screen.width / prev_width,
            rect.y * Screen.height / prev_height,
            rect.width * Screen.width / prev_width,
            rect.height * Screen.height / prev_height
        );
    }

    private Vector2 ScaleVec(Vector2 vec)
    {
        return new Vector2(
            vec.x * Screen.width / prev_width,
            vec.y * Screen.height / prev_height
        );
    }
    #endregion

    private void OnDestroy(){ 
        lButton.action.performed -= LeftClick;
        spaceBar.action.performed -= SpaceBar;
        rKey.action.performed -= RKey;
        cKey.action.performed -= CKey;
        hKey.action.performed -= HKey;
        arrowKeys[0].action.started -= LeftArrowPressed;
        arrowKeys[0].action.canceled -= LeftArrowReleased;
        arrowKeys[1].action.started -= RightArrowPressed;
        arrowKeys[1].action.canceled -= RightArrowReleased;
        arrowKeys[2].action.started -= UpArrowPressed;
        arrowKeys[2].action.canceled -= UpArrowReleased;
        arrowKeys[3].action.started -= DownArrowPressed;
        arrowKeys[3].action.canceled -= DownArrowReleased;
    }
    private void OnEnable() {
        lButton.action.Enable();
        spaceBar.action.Enable();
        rKey.action.Enable();
        cKey.action.Enable();
        hKey.action.Enable();
        arrowKeys[0].action.Enable();
        arrowKeys[1].action.Enable();
        arrowKeys[2].action.Enable();
        arrowKeys[3].action.Enable();
    }
    private void OnDisable() {
        lButton.action.Disable();
        spaceBar.action.Disable();
        rKey.action.Disable();
        cKey.action.Disable();
        hKey.action.Disable();
        arrowKeys[0].action.Disable();
        arrowKeys[1].action.Disable();
        arrowKeys[2].action.Disable();
        arrowKeys[3].action.Disable();
    }


    private void Awake()
    {
        lButton.action.performed += LeftClick;
        spaceBar.action.performed += SpaceBar;
        rKey.action.performed += RKey;
        cKey.action.performed += CKey;
        hKey.action.performed += HKey;
        arrowKeys[0].action.started += LeftArrowPressed;
        arrowKeys[0].action.canceled += LeftArrowReleased;
        arrowKeys[1].action.started += RightArrowPressed;
        arrowKeys[1].action.canceled += RightArrowReleased;
        arrowKeys[2].action.started += UpArrowPressed;
        arrowKeys[2].action.canceled += UpArrowReleased;
        arrowKeys[3].action.started += DownArrowPressed;
        arrowKeys[3].action.canceled += DownArrowReleased;
        props = new Dictionary<string, bool>();
    }


    private void FixedUpdate()
    {
        if(Screen.width != prev_width || Screen.height != prev_height)
        {
            if(GameManager.instance.sceneManager.activeSceneName == "Title") PrepareMenu();
            if(GameManager.instance.sceneManager.activeSceneName == "Game") StartGame();
            if(GameManager.instance.sceneManager.activeSceneName == "Ranking") ShowRanking();
            prev_width = Screen.width;
            prev_height = Screen.height;
        }

        if(buttonAreas != null){
            var pointer = Pointer.current;
            if (pointer != null){

                Vector2 mousePosition = pointer.position.ReadValue();
                foreach (var i in buttonAreas.Values)
                {
                    if(i.actionType == ActionType.Hold)
                    {
                        if (i.Contains(mousePosition) && Mouse.current.leftButton.isPressed)
                        {
                            i.DoAction();
                        }
                    }
                }
            }

            foreach (var i in buttonAreas.Values)
            {
                Vector2 s = i.area.max, t = i.area.min;
                Debug.DrawLine(new Vector3(s.x, s.y, 0), new Vector3(s.x, t.y, 0), Color.red);
                Debug.DrawLine(new Vector3(s.x, s.y, 0), new Vector3(t.x, s.y, 0), Color.red);
                Debug.DrawLine(new Vector3(t.x, t.y, 0), new Vector3(t.x, s.y, 0), Color.red);
                Debug.DrawLine(new Vector3(t.x, t.y, 0), new Vector3(s.x, t.y, 0), Color.red);
            }
        }

        if(GameManager.instance.sceneManager.activeSceneName == "Title")
        {
            
        }
        
        if(GameManager.instance.sceneManager.activeSceneName == "Game"){
            if (GameManager.instance.isCleared)
            {
                
            }
            else
            {
                timeSinceLastHold += Time.fixedDeltaTime;

                if(GameManager.instance.objManager.ControllingObj != null && props["isRotateMode"])
                {
                    Vector3 wpos = GameManager.instance.objManager.ControllingObj.transform.position;
                    Vector3 spos = Camera.main.WorldToScreenPoint(wpos);
                    Vector2 pos = new Vector2(spos.x, spos.y);
                    Vector2 size = new Vector2(100, 100);
                    string[] directions = { "Right", "Left", "Front", "Back" };
                    float[] dx = { 100, -100, 0, 0 }, dy = { -340, -340, -240, -440 };
                    //float[] dx = { 100, -100, 0, 0 }, dy = { 0, 0, 100, -100 };
                    for(int i = 0; i < 4; i++)
                    {
                        SetButtonArea("RotateObj"+directions[i], ToRect(ScaleVec(new Vector2(dx[i], dy[i]))+new Vector2(Screen.width/2, Screen.height/2), ScaleVec(size)));
                        //Debug.Log(ToRect(new Vector2(dx[i], dy[i])+new Vector2(960, 540), size));
                        GameManager.instance.uiManager.SetUIPlace(UIType.Button, "Rotate"+directions[i], new Vector2(dx[i], dy[i]));
                        /*
                        buttonAreas["RotateObj"+directions[i]].setArea(ToRect(pos + new Vector2(dx[i], dy[i]), size));
                        rotateButtons[i].rectTransform.anchoredPosition = pos + new Vector2(dx[i], dy[i]) - new Vector2(960, 540);
                        //*/
                        GameManager.instance.uiManager.SetUIActive(UIType.Button, "Rotate"+directions[i], true);
                        //GameManager.instance.uiManager.SetUISize(UIType.Button, "Rotate"+directions[i], size);
                    }
                    //Debug.Log(pos+" "+size+" "+ToRect(pos, size));
                }
                else
                {
                    string[] directions = { "Right", "Left", "Front", "Back" };
                    for(int i = 0; i < 4; i++)
                    {
                        buttonAreas["RotateObj"+directions[i]].setArea(new Rect());
                        GameManager.instance.uiManager.SetUIActive(UIType.Button, "Rotate"+directions[i], false);
                    }
                }

                if (props["isRotateMode"])
                {
                    if(props["isLeftArrowPressed"]){
                        ActionRotateObjLeft();
                    }
                    if(props["isRightArrowPressed"]){
                        ActionRotateObjRight();
                    }
                    if(props["isUpArrowPressed"]){
                        ActionRotateObjBack();
                    }
                    if(props["isDownArrowPressed"]){
                        ActionRotateObjFront();
                    }
                }
                else
                {
                    if(GameManager.instance.objManager.ControllingObj != null){
                        if(props["isLeftArrowPressed"]){
                            ActionMoveObjLeft();
                        }
                        if(props["isRightArrowPressed"]){
                            ActionMoveObjRight();
                        }
                        if(props["isUpArrowPressed"]){
                            ActionMoveObjFront();
                        }
                        if(props["isDownArrowPressed"]){
                            ActionMoveObjBack();
                        }
                    }
                }
            }
        }
    }

    public void SetButtonArea(string name, Rect area)
    {
        if (buttonAreas.ContainsKey(name))
        {
            Debug.Log(name + " " + area);
            buttonAreas[name].setArea(area);
        }
    }

    public void PrepareMenu()
    {
        buttonAreas = new Dictionary<string, ButtonArea>
        {
            { "StartGame", new ButtonArea(ScaleRect(new Rect(710, 140, 200, 200)), ActionStartGame) },
            { "ToRanking", new ButtonArea(ScaleRect(new Rect(1010, 140, 200, 200)), ActionToRanking) },
            { "IncreaseColorCount", new ButtonArea(ScaleRect(new Rect(1122.5f, 572.5f, 75, 75)), ActionIncreaseColorCount) },
            { "DecreaseColorCount", new ButtonArea(ScaleRect(new Rect(1122.5f, 432.5f, 75, 75)), ActionDecreaseColorCount) },
            { "IncreaseTypeCount", new ButtonArea(ScaleRect(new Rect(922.5f, 572.5f, 75, 75)), ActionIncreaseTypeCount) },
            { "DecreaseTypeCount", new ButtonArea(ScaleRect(new Rect(922.5f, 432.5f, 75, 75)), ActionDecreaseTypeCount) },
            { "ToggleAnimalMode", new ButtonArea(ScaleRect(new Rect(1790, 30, 100, 100)), ActionToggleAnimalMode) }
        };
        props["isCameraMoving"] = false;
        props["isRotateMode"] = false;
        props["isAnimalMode"] = GameManager.instance.isOnlyAnimals;
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
        GameManager.instance.uiManager.SetUIText("ColorCount",GameManager.instance.colorCount.ToString());
        Debug.Log("InputManager PrepareMenu");
    }

    public void StartGame()
    {
        buttonAreas = new Dictionary<string, ButtonArea>
        {
            { "SwitchRotateMode", new ButtonArea(ScaleRect( new Rect(1710, 840, 200, 200)), ActionSwitchRotateMode, 1) },
            { "SwitchCameraMove", new ButtonArea(ScaleRect( new Rect(1710, 620, 200, 200)), ActionSwitchCameraMove, 1) },
            { "DropObject", new ButtonArea(ScaleRect( new Rect(1710, 380, 200, 200)), ActionDropObject, 1) },
            { "MoveObject", new ButtonArea(ScaleRect( new Rect(0, 0, 1920, 1080)), ActionMoveObject) },
            { "RotateObjRight", new ButtonArea(ScaleRect( new Rect()), ActionRotateObjRight, 2, ActionType.Hold) },
            { "RotateObjLeft", new ButtonArea(ScaleRect( new Rect()), ActionRotateObjLeft, 2, ActionType.Hold) },
            { "RotateObjFront", new ButtonArea(ScaleRect( new Rect()), ActionRotateObjFront, 2, ActionType.Hold) },
            { "RotateObjBack", new ButtonArea(ScaleRect( new Rect()), ActionRotateObjBack, 2, ActionType.Hold) },
            { "RestartGame", new ButtonArea(ScaleRect( new Rect()), ActionRestartGame, 1) },
            { "TitleButton", new ButtonArea(ScaleRect( new Rect()), ActionToTitle, 1) },
            { "HoldObj", new ButtonArea(ScaleRect( new Rect(1535, 15, 150, 150)), ActionHold, 1) },
        };
        props["isCameraMoving"] = false;
        props["isRotateMode"] = false;
        props["isLeftArrowPressed"] = false;
        props["isRightArrowPressed"] = false;
        props["isUpArrowPressed"] = false;
        props["isDownArrowPressed"] = false;
        props["isAnimalMode"] = GameManager.instance.isOnlyAnimals;
        Debug.Log("InputManager StartGame");
    }

    public void ShowRanking()
    {
        buttonAreas = new Dictionary<string, ButtonArea>
        {
            { "NextPage", new ButtonArea(ScaleRect(new Rect(1690, 890, 140, 140)), GameManager.instance.scoreManager.NextPage) },
            { "PreviousPage", new ButtonArea(ScaleRect(new Rect(90, 890, 140, 140)), GameManager.instance.scoreManager.PreviousPage) },
            { "TitleButton", new ButtonArea(ScaleRect(new Rect(1710, 70, 100, 100)), ActionToTitle, 1) }
        };
        Debug.Log("InputManager ShowRanking");
    }

    public Rect ToRect(Vector2 pos, Vector2 size)
    {
        return new Rect(pos - size / 2, size);
    }


    #region Key Inputs
    private void LeftClick(InputAction.CallbackContext context)
    {
        if(buttonAreas == null) return;
        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 mousePosition = pointer.position.ReadValue();
        //Debug.Log("Mouse Position: " + mousePosition);
        string index = "";
        int maxPriority = int.MinValue;
        foreach (var i in buttonAreas)
        {
            ButtonArea buttonArea = i.Value;
            //Debug.Log(i.Key+" "+buttonArea.area+" "+buttonArea.Contains(mousePosition));
            if(buttonArea.actionType == ActionType.Hold) continue;
            if (buttonArea.Contains(mousePosition))
            {
                if (buttonArea.priority > maxPriority)
                {
                    maxPriority = buttonArea.priority;
                    index = i.Key;
                }
            }
        }
        if (index == "") return;
        buttonAreas[index].DoAction();
        Debug.Log(index);
    }

    private void SpaceBar(InputAction.CallbackContext context)
    {
        Debug.Log("SpaceBar Pressed");
        if(GameManager.instance.sceneManager.activeSceneName == "Game") ActionDropObject();
        if(GameManager.instance.sceneManager.activeSceneName == "Title") ActionStartGame();
    }

    private void RKey(InputAction.CallbackContext context)
    {
        Debug.Log("RKey Pressed");
        if(GameManager.instance.sceneManager.activeSceneName == "Game") ActionSwitchRotateMode();
        if(GameManager.instance.sceneManager.activeSceneName == "Title") ActionToRanking();
        if(GameManager.instance.sceneManager.activeSceneName == "Ranking") ActionToTitle();
    }

    private void CKey(InputAction.CallbackContext context)
    {
        Debug.Log("CKey Pressed");
        if(GameManager.instance.sceneManager.activeSceneName == "Game") ActionSwitchCameraMove();
    }

    private void HKey(InputAction.CallbackContext context)
    {
        Debug.Log("HKey Pressed");
        if(GameManager.instance.sceneManager.activeSceneName == "Game") ActionHold();
    }

    private void LeftArrowPressed(InputAction.CallbackContext context)
    {
        props["isLeftArrowPressed"] = true;
    }

    private void LeftArrowReleased(InputAction.CallbackContext context)
    {
        props["isLeftArrowPressed"] = false;
    }

    private void RightArrowPressed(InputAction.CallbackContext context)
    {
        props["isRightArrowPressed"] = true;
    }
    private void RightArrowReleased(InputAction.CallbackContext context)
    {
        props["isRightArrowPressed"] = false;
    }

    private void UpArrowPressed(InputAction.CallbackContext context)
    {
        props["isUpArrowPressed"] = true;
    }
    private void UpArrowReleased(InputAction.CallbackContext context)
    {
        props["isUpArrowPressed"] = false;
    }

    private void DownArrowPressed(InputAction.CallbackContext context)
    {
        props["isDownArrowPressed"] = true;
    }
    private void DownArrowReleased(InputAction.CallbackContext context)
    {
        props["isDownArrowPressed"] = false;
    }
    
    #endregion

    //******************************************************************************
    //                          ここから下は各ボタンのアクション
    //******************************************************************************
    #region GameActions

    public void ActionMoveObject()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"] || props["isRotateMode"]) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up,new Vector3(0, GameManager.instance.STAGE_HEIGHT, 0));
        float dist;
        //GenerateObject(0, new Vector3(0,stageHeight,0));
        if (plane.Raycast(ray, out dist))
        {
            
            Vector3 hitPoint = ray.GetPoint(dist);
            Debug.Log(hitPoint);
            if(Mathf.Abs(hitPoint.x) >= GameManager.instance.STAGE_WIDTH-12.0f || Mathf.Abs(hitPoint.z) >= GameManager.instance.STAGE_WIDTH-12.0f){
                //Debug.Log(hitPoint);
                if(Mathf.Abs(hitPoint.x) >= GameManager.instance.STAGE_WIDTH-12.0f && Mathf.Abs(hitPoint.x)*0.65f <= GameManager.instance.STAGE_WIDTH-12.0f) hitPoint.x = (GameManager.instance.STAGE_WIDTH-13f)*Mathf.Sign(hitPoint.x);
                if(Mathf.Abs(hitPoint.z) >= GameManager.instance.STAGE_WIDTH-12.0f && Mathf.Abs(hitPoint.z)*0.65f <= GameManager.instance.STAGE_WIDTH-12.0f) hitPoint.z = (GameManager.instance.STAGE_WIDTH-13f)*Mathf.Sign(hitPoint.z);
                //Debug.Log(hitPoint);
            }
            if (Mathf.Abs(hitPoint.x) < GameManager.instance.STAGE_WIDTH - 12.0f && Mathf.Abs(hitPoint.z) < GameManager.instance.STAGE_WIDTH - 12.0f)
            {
                if (GameManager.instance.objManager.isObjMoving)
                {
                    GameManager.instance.objManager.ControllingObj.destination = hitPoint;
                    Debug.Log("Set Destination: " + hitPoint + " " + GameManager.instance.objManager.ControllingObj.destination);
                    if(GameManager.instance.predictor != null) GameManager.instance.predictor.RemovePredict();
                }
            }
            else if(GameManager.instance.objManager.splitQueue.Count == 0)
            {
                //Debug.Log("Out of range");
                if(Mathf.Abs(hitPoint.x) >= GameManager.instance.STAGE_WIDTH + 12.0f || Mathf.Abs(hitPoint.z) >= GameManager.instance.STAGE_WIDTH + 12.0f)
                {
                    return;
                    GameManager.instance.objManager.DropObj();
                }
            }
        }
    }

    public void ActionDropObject()
    {
        GameManager.instance.objManager.DropObj();
    }

    public void ActionSwitchCameraMove()
    {
        props["isCameraMoving"] = !props["isCameraMoving"];
        props["isRotateMode"] = false;
    }

    public void ActionSwitchRotateMode()
    {
        props["isRotateMode"] = !props["isRotateMode"];
        props["isCameraMoving"] = false;
    }
    
    public void ActionRotateObjRight()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.RotateRight();
    }

    public void ActionRotateObjLeft()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.RotateLeft();
        GameManager.instance.objManager.ControllingObj.Predict();
    }

    public void ActionRotateObjFront()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.RotateFront();
    }

    public void ActionRotateObjBack()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.RotateBack();
    }

    public void ActionMoveObjRight()
    {
        Vector3 horizontalForward =
            Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);

        if (horizontalForward.sqrMagnitude > 0.0001f)
        {
            horizontalForward.Normalize();
        }

        Vector3 right = Vector3.Cross(Vector3.up, horizontalForward);
        GameManager.instance.objManager.ControllingObj.destination += right;
        GameManager.instance.objManager.ControllingObj.destination = CalcPositionUtils.CalcValidPos(GameManager.instance.objManager.ControllingObj.destination, GameManager.instance.objManager.ControllingObj.transform.localScale);
        GameManager.instance.objManager.ControllingObj.Predict();
        GameManager.instance.objManager.ControllingObj.destination.y = GameManager.instance.STAGE_HEIGHT;
    }
    public void ActionMoveObjLeft()
    {
        Vector3 horizontalForward =
            Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);

        if (horizontalForward.sqrMagnitude > 0.0001f)
        {
            horizontalForward.Normalize();
        }

        Vector3 right = Vector3.Cross(Vector3.up, horizontalForward);
        GameManager.instance.objManager.ControllingObj.destination -= right;
        GameManager.instance.objManager.ControllingObj.destination = CalcPositionUtils.CalcValidPos(GameManager.instance.objManager.ControllingObj.destination, GameManager.instance.objManager.ControllingObj.transform.localScale);
        GameManager.instance.objManager.ControllingObj.Predict();
        GameManager.instance.objManager.ControllingObj.destination.y = GameManager.instance.STAGE_HEIGHT;
    }

    public void ActionMoveObjFront()
    {
        Vector3 horizontalForward =
            Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);

        if (horizontalForward.sqrMagnitude > 0.0001f)
        {
            horizontalForward.Normalize();
        }

        GameManager.instance.objManager.ControllingObj.destination += horizontalForward;
        GameManager.instance.objManager.ControllingObj.destination = CalcPositionUtils.CalcValidPos(GameManager.instance.objManager.ControllingObj.destination, GameManager.instance.objManager.ControllingObj.transform.localScale);
        GameManager.instance.objManager.ControllingObj.Predict();
        GameManager.instance.objManager.ControllingObj.destination.y = GameManager.instance.STAGE_HEIGHT;
    }

    public void ActionMoveObjBack()
    {
        Vector3 horizontalForward =
            Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);

        if (horizontalForward.sqrMagnitude > 0.0001f)
        {
            horizontalForward.Normalize();
        }

        GameManager.instance.objManager.ControllingObj.destination -= horizontalForward;
        GameManager.instance.objManager.ControllingObj.destination = CalcPositionUtils.CalcValidPos(GameManager.instance.objManager.ControllingObj.destination, GameManager.instance.objManager.ControllingObj.transform.localScale);
        GameManager.instance.objManager.ControllingObj.Predict();
        GameManager.instance.objManager.ControllingObj.destination.y = GameManager.instance.STAGE_HEIGHT;
    }

    public void ActionRestartGame()
    {
        GameManager.instance.isCleared = false;
        GameManager.instance.objManager.ObjClearLineTouchingTimes.Clear();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void ActionToTitle()
    {
        GameManager.instance.isCleared = false;
        GameManager.instance.objManager.ObjClearLineTouchingTimes.Clear();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }
    
    public void ActionHold()
    {
        if(timeSinceLastHold < 0.5f) return;
        timeSinceLastHold = 0f;

        if(GameManager.instance.objManager.HoldObj.type == ObjType.None)
        {
            ObjData data = new ObjData(ObjType.None, -1)
            {
                type = GameManager.instance.objManager.ControllingObj.type,
                color = GameManager.instance.objManager.ControllingObj.color
            };
            GameManager.instance.objManager.HoldObj = data;
            Destroy(GameManager.instance.objManager.ControllingObj.gameObject);
            GameManager.instance.objManager.ControllingObj = null;
            GameManager.instance.objManager.isObjMoving = false;
        }
        else
        {
            ObjData data = new ObjData(ObjType.None, -1)
            {
                type = GameManager.instance.objManager.ControllingObj.type,
                color = GameManager.instance.objManager.ControllingObj.color
            };
            ObjData holdData = GameManager.instance.objManager.HoldObj;
            Destroy(GameManager.instance.objManager.ControllingObj.gameObject);
            GameManager.instance.objManager.ControllingObj = GameManager.instance.objManager.GenerateObject(new Vector3(0, GameManager.instance.STAGE_HEIGHT, 0),0,(int)holdData.type, holdData.color);
            //GameManager.instance.objManager.ControllingObj.Init();
            GameManager.instance.objManager.ControllingObj.Predict();
            GameManager.instance.objManager.HoldObj = data;
        }
    }
    #endregion

    #region MenuActions

    public void ActionStartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void ActionToRanking()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Ranking");
    }

    public void ActionIncreaseColorCount()
    {
        if(GameManager.instance.colorCount < GameManager.instance.MAX_COLOR_COUNT) GameManager.instance.colorCount++;
        GameManager.instance.uiManager.SetUIText("ColorCount",GameManager.instance.colorCount.ToString());
    }
    public void ActionDecreaseColorCount()
    {
        if(GameManager.instance.colorCount > 2) GameManager.instance.colorCount--;
        GameManager.instance.uiManager.SetUIText("ColorCount",GameManager.instance.colorCount.ToString());
    }

    public void ActionIncreaseTypeCount()
    {
        if(GameManager.instance.isOnlyAnimals) return;
        if(GameManager.instance.typeCount < GameManager.instance.MAX_TYPE_COUNT) GameManager.instance.typeCount++;
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
    }

    public void ActionDecreaseTypeCount()
    {
        if(GameManager.instance.isOnlyAnimals) return;
        if(GameManager.instance.typeCount > 2) GameManager.instance.typeCount--;
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
    }

    public void ActionToggleAnimalMode()
    {
        GameManager.instance.isOnlyAnimals = !GameManager.instance.isOnlyAnimals;
        props["isAnimalMode"] = GameManager.instance.isOnlyAnimals;
        GameManager.instance.typeCount = Mathf.Min(GameManager.instance.typeCount, GameManager.instance.isOnlyAnimals ? (int)ObjType.Animals - (int)ObjType.Geometric - 1 : GameManager.instance.MAX_TYPE_COUNT);
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
    }
    #endregion
}
