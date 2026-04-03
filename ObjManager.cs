using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ObjType
{
    None = -1,
    Capsule,
    Cube,
    decimalPrism,
    Sphere,
    Dolphin,
    Pyramid,
    [InspectorName("Don't Choose This")]
    Max
}

[System.Serializable]
public struct Animal
{
    public GameObject prefab;
    //public List<Material> materials;

    public ObjType type;

    public ObjType next;
}

[System.Serializable]
public class ObjectMaterial
{
    [ColorUsage(true, false)]
    public Color color;

    public Material material;

    public ObjectMaterial(Color color)
    {
        this.color = color;
    }
}

public class ObjManager : MonoBehaviour
{
    [Header("publicアクセスしたいだけ")]
    public bool isObjMoving;
    public Obj ControllingObj;
    public List<int> ObjClearLineTouchingTimes;

    [Header("SerializeField")]
    [SerializeField] private List<Animal> objAnimals;
    public List<Animal> animals;

    public List<ObjectMaterial> objMaterials;

    public List<Dictionary<int,int>> objColorCounter;

    [SerializeField] private Transform ObjParent;

    //先に定義しておく定数
    private static readonly int[] dx = { 10, 10, -10, -10 },dz = { 10, -10, 10, -10 }; // 分割後のオブジェクトの位置調整用
    
    private void FixedUpdate()
    {
        if (GameManager.instance.isCleared)
        {
            
        }
        else
        {
            for(int i = 0;i < ObjClearLineTouchingTimes.Count; i++)
            {
                if(ControllingObj != null && i == ControllingObj.id) continue;
                if(ObjClearLineTouchingTimes[i] >= 0) ObjClearLineTouchingTimes[i]++;
                if(ObjClearLineTouchingTimes[i] >= 180){
                    GameManager.instance.ClearGame();
                    break;
                }
            }
        }
    }

    public void StartGame()
    {
        ObjClearLineTouchingTimes = new List<int>();
        isObjMoving = false;
        ControllingObj = null;
        ObjParent = GameObject.Find("Objects' Parent").transform;

        GenerateMaterials();
        SetAnimals();

        //*
        for(int i = 0;i < GameManager.instance.colorCount*GameManager.instance.typeCount/2; i++)
        {
            GenerateObject(new Vector3(Random.Range(-GameManager.instance.STAGE_WIDTH,GameManager.instance.STAGE_WIDTH),GameManager.instance.STAGE_HEIGHT,Random.Range(-GameManager.instance.STAGE_WIDTH,GameManager.instance.STAGE_WIDTH)),0);
        }
        //*/
        
        Debug.Log("ObjManager StartGame");
    }

    private void GenerateMaterials()
    {
        foreach(var mat in objMaterials)
        {
            mat.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

            mat.material.SetColor("_BaseColor", mat.color);
            mat.material.SetFloat("_Metallic", 0f);
            mat.material.SetFloat("_Smoothness", 0.5f);
            mat.material.SetFloat("_Surface", 1);
            mat.material.SetOverrideTag("RenderType", "Transparent");
            mat.material.SetFloat("_Blend", 0);
            mat.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.material.SetInt("_ZWrite", 0);
            mat.material.SetFloat("_AlphaClip", 0);
            mat.material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.material.DisableKeyword("_ALPHATEST_ON");
            mat.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            mat.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }

    private void SetAnimals()
    {
        animals = new List<Animal>(objAnimals);
        animals.Sort((a, b) => a.type.CompareTo(b.type));
        objColorCounter = new List<Dictionary<int, int>>(animals.Count);
        for(int i = 0;i < Mathf.Min(animals.Count, GameManager.instance.typeCount); i++)
        {
            objColorCounter.Add(new Dictionary<int, int>());
            for(int j = 2;j < Mathf.Min(objMaterials.Count, GameManager.instance.colorCount + 2); j++)
            {
                objColorCounter[i].Add(j, 0);
            }
        }
    }

    public void ObjectTouchClearLine(int id)
    {
        //Debug.Log("Enter:"+id);
        if(id == ObjClearLineTouchingTimes.Count && isObjMoving) return;
        if(ObjClearLineTouchingTimes[id] < 0) ObjClearLineTouchingTimes[id] = 0;
    }

    public void ObjectUntouchClearLine(int id)
    {
        //Debug.Log("Exit:"+id);
        //if(id == ObjClearLineTouchingTimes.Count && isObjMoving) return;
        ObjClearLineTouchingTimes[id] = -1;
    }

    public void NextObj()
    {
        //Debug.Log("NextObj");
        if(GameManager.instance.isCleared || ControllingObj) return;
        int shape = Random.Range(0,GameManager.instance.typeCount);
        Debug.Log("NextObj : "+shape);
        var obj = GenerateObject(GetRandomPosInStage(),0,shape);
        ControllingObj = obj;
        ControllingObj.Predict();
        GameManager.instance.movecount++;
    }

    public void DropObj()
    {
        if(ControllingObj == null || GameManager.instance.isCleared) return;
        ControllingObj.Drop();
        ControllingObj = null;
        if(GameManager.instance.predictor != null) GameManager.instance.predictor.RemovePredict();
    }
    
    public void SplitObject(Vector3 pos, Obj first, Obj second)
    {
        if(GameManager.instance.isCleared || ObjClearLineTouchingTimes.Count > GameManager.instance.MAX_OBJECT_COUNT) return;
        GameManager.instance.comboCount++;
        GameManager.instance.scoreManager.AddScore(GameManager.instance.comboCount);
        GameManager.instance.timeSinceLastSplit = 0f;
        //Debug.Log("Split");
        ObjClearLineTouchingTimes[first.id] = -1;
        ObjClearLineTouchingTimes[second.id] = -1;
        //Debug.Log("first:"+first.id+" second:"+second.id);
        Destroy(first.gameObject);
        Destroy(second.gameObject);
        if(ObjClearLineTouchingTimes.Count == first.id+1) isObjMoving = false;
        int nextType = (int)animals[(int)first.type].next;
        if(nextType >= GameManager.instance.typeCount) nextType = GameManager.instance.typeCount-1;
        for (int i = 0; i < 2; i++)
        {
            Vector3 p = new Vector3(pos.x + dx[i], pos.y, pos.z + dz[i]);
            GenerateObject(p, first.splitCount+1, nextType).state = Obj.State.Finished;
            if(ObjClearLineTouchingTimes.Count >= GameManager.instance.MAX_OBJECT_COUNT) return;
        }
        for (int i = 2; i < 4; i++)
        {
            Vector3 p = new Vector3(pos.x + dx[i], pos.y, pos.z + dz[i]);
            GenerateObject(p, second.splitCount+1, nextType).state = Obj.State.Finished;
            if(ObjClearLineTouchingTimes.Count >= GameManager.instance.MAX_OBJECT_COUNT) return;
        }
    }

    public Obj GenerateObject(Vector3 pos, int splitCount, int type = -1)
    {
        if (type == -1)
        {
            type = Random.Range(0, GameManager.instance.typeCount);
        }
        var obj = Instantiate(animals[type].prefab, new Vector3(0,100,0), Quaternion.identity, ObjParent);
        /*
        float loc = Mathf.Pow(1.2f, level);
        Vector3 def = obj.transform.localScale;
        Vector3 scale = new Vector3(def.x*loc, def.y*loc, def.z*loc);
        obj.transform.localScale = scale;
        //*/
        Vector3 scale = obj.transform.localScale;
        pos = CalcGeneratePos(pos,scale);
        obj.transform.position = pos;
        var objObj = obj.GetComponent<Obj>();
        objObj.type = (ObjType)type;
        objObj.splitCount = splitCount;
        return objObj;
    }

    private Vector3 GetRandomPosInStage()
    {
        return new Vector3(Random.Range(0,GameManager.instance.STAGE_WIDTH*9)/10*(-1+Random.Range(0,2)),GameManager.instance.STAGE_HEIGHT,Random.Range(0,GameManager.instance.STAGE_WIDTH*9)/10*(-1+Random.Range(0,2)));
    }
    
    private Vector3 CalcGeneratePos(Vector3 pos,Vector3 scale)
    {
        if (Mathf.Abs(pos.x) > GameManager.instance.STAGE_WIDTH - scale.x / 2)
        {
            if (pos.x < 0) pos.x = -GameManager.instance.STAGE_WIDTH + scale.x / 2;
            else pos.x = GameManager.instance.STAGE_WIDTH - scale.x / 2;
        }
        if (Mathf.Abs(pos.z) > GameManager.instance.STAGE_WIDTH - scale.z / 2)
        {
            if (pos.z < 0) pos.z = -GameManager.instance.STAGE_WIDTH + scale.z / 2;
            else pos.z = GameManager.instance.STAGE_WIDTH - scale.z / 2;
        }
        return pos;
    }
}
