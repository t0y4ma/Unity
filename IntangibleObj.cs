using UnityEngine;

public class IntangibleObj : MonoBehaviour
{
    [SerializeField] private ObjType objType;
    [SerializeField] private int color;
    [SerializeField] private MeshRenderer mr;
    [SerializeField] private MeshFilter mf;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
    }

    public void SetObjType(ObjType objType)
    {
        this.objType = objType;
        if(objType == ObjType.None) return;
        
        if(objType >= ObjType.Animals)
        {
            mf.mesh = GameManager.instance.objManager.objMeshs[(int)objType-2];
        }
        else if(objType >= ObjType.Geometric){
            mf.mesh = GameManager.instance.objManager.objMeshs[(int)objType-1];
        }
        else
        {
            mf.mesh = GameManager.instance.objManager.objMeshs[(int)objType];
        }
        switch (objType)
        {
            case ObjType.None:
            case ObjType.Capsule:
            case ObjType.Sphere:
            case ObjType.Cube:
                transform.localScale = new Vector3(3.0f,3.0f,3.0f);
                break;
            case ObjType.Pyramid:
                transform.localScale = new Vector3(2.4f,2.4f,2.4f);
                break;
            case ObjType.LongPrism:
                transform.localScale = new Vector3(1.0f,1.0f,1.0f);
                break;
            case ObjType.Mammoth:
            case ObjType.Snake:
                transform.localScale = new Vector3(9.0f,9.0f,9.0f);
                break;
            case ObjType.Dolphin:
            case ObjType.Hamster:
            case ObjType.Giraffe:
            case ObjType.Bomb:
                transform.localScale = new Vector3(6.0f,6.0f,6.0f);
                break;
        }
        switch (objType)
        {
            case ObjType.Pyramid:
            case ObjType.LongPrism:
            case ObjType.Bomb:
            case ObjType.None:
            case ObjType.Capsule:
            case ObjType.Sphere:
            case ObjType.Cube:
            case ObjType.Hamster:
                transform.eulerAngles = new Vector3(0.0f,0.0f,0.0f);
                break;
            case ObjType.Mammoth:
                transform.eulerAngles = new Vector3(270.0f,0.0f,0.0f);
                break;
            case ObjType.Dolphin:
                transform.eulerAngles = new Vector3(270.0f,90.0f,180.0f);
                break;
            case ObjType.Giraffe:
                transform.eulerAngles = new Vector3(0.0f,90.0f,0.0f);
                break;
        }
    }

    public void SetColor(int color)
    {
        this.color = color;
        if(objType == ObjType.Bomb)
        {
            mr.material = GameManager.instance.objManager.BombMaterial;
            return;
        }
        if(color == -1) return;
        Debug.Log("SetColor : "+color);
        mr.material = GameManager.instance.objManager.objMaterials[color].material;
    }

    public void SetObjData(ObjData data)
    {
        SetObjType(data.type);
        SetColor(data.color);
    }
}
