using System;
using System.Collections.Generic;
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

    public Dictionary<string, bool> props;



    private void OnDestroy() => lButton.action.performed -= LeftClick;
    private void OnEnable() => lButton.action.Enable();
    private void OnDisable() => lButton.action.Disable();


    private void Awake()
    {
        lButton.action.performed += LeftClick;
        props = new Dictionary<string, bool>();
    }


    private void FixedUpdate()
    {
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

        if(GameManager.instance.sceneManager.activeSceneName == "Title")
        {
            
        }
        
        if(GameManager.instance.sceneManager.activeSceneName == "Game"){
            if (GameManager.instance.isCleared)
            {
                
            }
            else
            {
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
                        SetButtonArea("RotateObj"+directions[i], ToRect(new Vector2(dx[i], dy[i])+new Vector2(960, 540), size));
                        //Debug.Log(ToRect(new Vector2(dx[i], dy[i])+new Vector2(960, 540), size));
                        GameManager.instance.uiManager.SetUIPlace(UIType.Button, "Rotate"+directions[i], new Vector2(dx[i], dy[i]));
                        /*
                        buttonAreas["RotateObj"+directions[i]].setArea(ToRect(pos + new Vector2(dx[i], dy[i]), size));
                        rotateButtons[i].rectTransform.anchoredPosition = pos + new Vector2(dx[i], dy[i]) - new Vector2(960, 540);
                        //*/
                        GameManager.instance.uiManager.SetUIActive(UIType.Button, "Rotate"+directions[i], true);
                        GameManager.instance.uiManager.SetUISize(UIType.Button, "Rotate"+directions[i], size);
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
            {"StartGame", new ButtonArea(new Rect(860, 190, 200, 100), ActionStartGame) },
            { "IncreaseColorCount", new ButtonArea(new Rect(1122.5f, 572.5f, 75, 75), ActionIncreaseColorCount) },
            { "DecreaseColorCount", new ButtonArea(new Rect(1122.5f, 432.5f, 75, 75), ActionDecreaseColorCount) },
            { "IncreaseTypeCount", new ButtonArea(new Rect(922.5f, 572.5f, 75, 75), ActionIncreaseTypeCount) },
            { "DecreaseTypeCount", new ButtonArea(new Rect(922.5f, 432.5f, 75, 75), ActionDecreaseTypeCount) }
        };
        props["isCameraMoving"] = false;
        props["isRotateMode"] = false;
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
        GameManager.instance.uiManager.SetUIText("ColorCount",GameManager.instance.colorCount.ToString());
        Debug.Log("InputManager PrepareMenu");
    }

    public void StartGame()
    {
        buttonAreas = new Dictionary<string, ButtonArea>
        {
            { "SwitchCameraMove", new ButtonArea(new Rect(1710, 440, 200, 200), ActionSwitchCameraMove, 1) },
            { "DropObject", new ButtonArea(new Rect(0, 0, 1920, 1080), ActionDropObject) },
            { "SwitchRotateMode", new ButtonArea(new Rect(1710, 740, 200, 200), ActionSwitchRotateMode, 1) },
            { "RotateObjRight", new ButtonArea(new Rect(), ActionRotateObjRight, 2, ActionType.Hold) },
            { "RotateObjLeft", new ButtonArea(new Rect(), ActionRotateObjLeft, 2, ActionType.Hold) },
            { "RotateObjFront", new ButtonArea(new Rect(), ActionRotateObjFront, 2, ActionType.Hold) },
            { "RotateObjBack", new ButtonArea(new Rect(), ActionRotateObjBack, 2, ActionType.Hold) },
            { "RestartGame", new ButtonArea(new Rect(), ActionRestartGame, 1) },
            { "TitleButton", new ButtonArea(new Rect(), ActionToTitle, 1) }
        };
        props["isCameraMoving"] = false;
        props["isRotateMode"] = false;
        Debug.Log("InputManager StartGame");
    }

    public Rect ToRect(Vector2 pos, Vector2 size)
    {
        return new Rect(pos - size / 2, size);
    }

    private void LeftClick(InputAction.CallbackContext context)
    {
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
        //Debug.Log(index);
    }


    //******************************************************************************
    //                          ここから下は各ボタンのアクション
    //******************************************************************************
    #region GameActions
    public void ActionDropObject()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"] || props["isRotateMode"]) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up,new Vector3(0, GameManager.instance.STAGE_HEIGHT, 0));
        float dist;
        //GenerateObject(0, new Vector3(0,stageHeight,0));
        if (plane.Raycast(ray, out dist))
        {
            // ちょっと範囲外なら補正するようにする
            
            Vector3 hitPoint = ray.GetPoint(dist);
            //Debug.Log(hitPoint);
            if(Mathf.Abs(hitPoint.x) >= GameManager.instance.STAGE_WIDTH-10.0f || Mathf.Abs(hitPoint.z) >= GameManager.instance.STAGE_WIDTH-10.0f){
                //Debug.Log(hitPoint);
                if(Mathf.Abs(hitPoint.x) >= GameManager.instance.STAGE_WIDTH-10.0f && Mathf.Abs(hitPoint.x)*0.65f <= GameManager.instance.STAGE_WIDTH-10.0f) hitPoint.x = (GameManager.instance.STAGE_WIDTH-11f)*Mathf.Sign(hitPoint.x);
                if(Mathf.Abs(hitPoint.z) >= GameManager.instance.STAGE_WIDTH-10.0f && Mathf.Abs(hitPoint.z)*0.65f <= GameManager.instance.STAGE_WIDTH-10.0f) hitPoint.z = (GameManager.instance.STAGE_WIDTH-11f)*Mathf.Sign(hitPoint.z);
                //Debug.Log(hitPoint);
            }
            if (Mathf.Abs(hitPoint.x) < GameManager.instance.STAGE_WIDTH - 10.0f && Mathf.Abs(hitPoint.z) < GameManager.instance.STAGE_WIDTH - 10.0f)
            {
                if (GameManager.instance.objManager.isObjMoving)
                {
                    GameManager.instance.objManager.ControllingObj.destination = hitPoint;
                    if(GameManager.instance.predictor != null) GameManager.instance.predictor.RemovePredict();
                }
            }
            else
            {
                //Debug.Log("Out of range");
                if(Mathf.Abs(hitPoint.x) >= GameManager.instance.STAGE_WIDTH + 10.0f || Mathf.Abs(hitPoint.z) >= GameManager.instance.STAGE_WIDTH + 10.0f)
                {
                    GameManager.instance.objManager.DropObj();
                }
            }
        }
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

        GameManager.instance.objManager.ControllingObj.transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, Vector3.up, GameManager.instance.ROTATE_SPEED);
        GameManager.instance.objManager.ControllingObj.Predict();
    }

    public void ActionRotateObjLeft()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, Vector3.up, -GameManager.instance.ROTATE_SPEED);
        GameManager.instance.objManager.ControllingObj.Predict();
    }

    public void ActionRotateObjFront()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, GameManager.instance.objManager.ControllingObj.transform.right, GameManager.instance.ROTATE_SPEED);
        GameManager.instance.objManager.ControllingObj.Predict();
    }

    public void ActionRotateObjBack()
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null) return;
        if(props["isCameraMoving"]) return;

        GameManager.instance.objManager.ControllingObj.transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, GameManager.instance.objManager.ControllingObj.transform.right, -GameManager.instance.ROTATE_SPEED);
        GameManager.instance.objManager.ControllingObj.Predict();
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
    #endregion

    #region MenuActions

    public void ActionStartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
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
        if(GameManager.instance.typeCount < GameManager.instance.MAX_TYPE_COUNT) GameManager.instance.typeCount++;
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
    }

    public void ActionDecreaseTypeCount()
    {
        if(GameManager.instance.typeCount > 2) GameManager.instance.typeCount--;
        GameManager.instance.uiManager.SetUIText("TypeCount",GameManager.instance.typeCount.ToString());
    }

    #endregion
}
