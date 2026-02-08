using UnityEngine;

public class ClearLine : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnTriggerEnter(Collider other)
    {
        Obj obj = other.gameObject.GetComponent<Obj>();
        if(obj != null) GameManager.instance.objControlManager.ObjectTouchClearLine(obj.id);
    }

    public void OnTriggerExit(Collider other)
    {
        Obj obj= other.gameObject.GetComponent<Obj>();
        if(obj != null) GameManager.instance.objControlManager.ObjectUntouchClearLine(obj.id);
    }
}
