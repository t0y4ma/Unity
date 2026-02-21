using UnityEngine;

public class ClearLine : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnTriggerEnter(Collider other)
    {
        var obj = other.gameObject.GetComponent<ObjCollideTrigger>();
        Debug.Log("ClearLine Triggered by "+other.gameObject.name);
        if(obj != null && obj.obj.id >= 0) GameManager.instance.objManager.ObjectTouchClearLine(obj.obj.id);
    }

    public void OnTriggerExit(Collider other)
    {
        var obj= other.gameObject.GetComponent<ObjCollideTrigger>();
        Debug.Log("ClearLine UnTriggered by "+other.gameObject.name);
        if(obj != null && obj.obj.id >= 0) GameManager.instance.objManager.ObjectUntouchClearLine(obj.obj.id);
    }
}
