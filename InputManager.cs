using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    
    public bool isCameraMoving;
    [SerializeField] private UIDocument uiDocument;

    [SerializeField] private InputActionProperty lButton;
    private void OnDestroy() => lButton.action.performed -= PressAction;
    private void OnEnable() => lButton.action.Enable();
    private void OnDisable() => lButton.action.Disable();


    private void Awake()
    {
        lButton.action.performed += PressAction;
        if (uiDocument == null) { return; }
        VisualElement root = uiDocument.rootVisualElement;
        Button button = root.Q<Button>();
        // ボタンをヒット対象にする
        UIToolkitRaycastChecker.RegisterBlockingElement(button);
    }

    private void PressAction(InputAction.CallbackContext context)
    {
        if(GameManager.instance.isCleared || GameManager.instance.objManager.ControllingObj == null || isCameraMoving) return;
        var pointer = Pointer.current;
        if (pointer == null) return;
        if(UIToolkitRaycastChecker.IsHoverUI()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up,new Vector3(0, GameManager.instance.stageHeight, 0));
        float dist;
        //GenerateObject(0, new Vector3(0,stageHeight,0));
        if (plane.Raycast(ray, out dist))
        {
            // ちょっと範囲外なら補正するようにする
            
            Vector3 hitPoint = ray.GetPoint(dist);
            //Debug.Log(hitPoint);
            if(Mathf.Abs(hitPoint.x) >= GameManager.instance.stagesize-10.0f || Mathf.Abs(hitPoint.z) >= GameManager.instance.stagesize-10.0f){
                //Debug.Log(hitPoint);
                if(Mathf.Abs(hitPoint.x) >= GameManager.instance.stagesize-10.0f && Mathf.Abs(hitPoint.x)*0.65f <= GameManager.instance.stagesize-10.0f) hitPoint.x = (GameManager.instance.stagesize-11f)*Mathf.Sign(hitPoint.x);
                if(Mathf.Abs(hitPoint.z) >= GameManager.instance.stagesize-10.0f && Mathf.Abs(hitPoint.z)*0.65f <= GameManager.instance.stagesize-10.0f) hitPoint.z = (GameManager.instance.stagesize-11f)*Mathf.Sign(hitPoint.z);
                //Debug.Log(hitPoint);
            }
            if (Mathf.Abs(hitPoint.x) < GameManager.instance.stagesize - 10.0f && Mathf.Abs(hitPoint.z) < GameManager.instance.stagesize - 10.0f)
            {
                if (GameManager.instance.objManager.isObjMoving)
                {
                    GameManager.instance.objManager.ControllingObj.destination = hitPoint;
                    GameManager.instance.predictor.RemovePredict();
                }
            }
            else
            {
                //Debug.Log("Out of range");
                if(Mathf.Abs(hitPoint.x) >= GameManager.instance.stagesize + 10.0f || Mathf.Abs(hitPoint.z) >= GameManager.instance.stagesize + 10.0f)
                {
                    GameManager.instance.objManager.DropObj();
                }
            }
        }
    }


    public void SwitchCameraMove()
    {
        isCameraMoving = !isCameraMoving;
    }
}

//コピペ from https://gist.github.com/NickMercer/60b13551aaf8e3b86129c6a3ee35bc67
public static class UIToolkitRaycastChecker
{
    private static HashSet<VisualElement> _blockingElements = new HashSet<VisualElement>();

    public static void RegisterBlockingElement(VisualElement blockingElement) =>
        _blockingElements.Add(blockingElement);

    public static bool IsBlockingRaycasts(VisualElement element)
    {
        return _blockingElements.Contains(element) &&
               element.visible;
    }

    public static bool IsHoverUI()
    {
        foreach (var element in _blockingElements)
        {
            if (IsBlockingRaycasts(element) == false)
                continue;

            if (ContainsMouse(element) && element.visible)
            {
                return true;
            }
        }

        return false;
    }

    public static VisualElement GetHitElement()
    {
        foreach (var element in _blockingElements)
        {
            if (IsBlockingRaycasts(element) == false)
                continue;

            if (ContainsMouse(element) && element.visible)
            {
                return element;
            }
        }
        return null;
    }

    private static bool ContainsMouse(VisualElement element)
    {
        // Nullぽケアで拡張
        try
        {
            var mousePosition = Mouse.current.position.ReadValue();
            var scaledMousePosition = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height);

            var flippedPosition = new Vector2(scaledMousePosition.x, 1 - scaledMousePosition.y);
            // Null になることがある
            var adjustedPosition = flippedPosition * element.panel.visualTree.layout.size;

            var localPosition = element.WorldToLocal(adjustedPosition);

            return element.ContainsPoint(localPosition);
        }
        catch
        {
            return false;
        }
    }
}