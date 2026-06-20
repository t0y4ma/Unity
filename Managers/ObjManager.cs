
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

#region Structs and Enums
[Serializable]
public enum ObjType
{
    None = -1,
    Capsule,
    Cube,
    LongPrism,
    Sphere,
    Pyramid,
    Geometric,
    Dolphin,
    Mammoth,
    Giraffe,
    Hamster,
    Snake,
    Animals,
    Bomb,
    [InspectorName("Don't Choose This")]
    Max
}

[Serializable]
public struct Animal
{
    public GameObject prefab;
    //public List<Material> materials;

    public ObjType type;

    public ObjType next;
}

[Serializable]
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

[Serializable]
public class ObjData
{
    public ObjType type;
    public int color;

    public ObjData(ObjType type, int color)
    {
        this.type = type;
        this.color = color;
    }
}

#endregion

public class ObjManager : MonoBehaviour
{
    [Header("publicアクセスしたいだけ")]
    public bool isObjMoving;
    public int countAfterSplit = 4;
    public Obj ControllingObj;
    public List<int> ObjClearLineTouchingTimes;
    public List<int> ObjClearLineTouchingColliderCounts;

    [Header("SerializeField")]
    public List<Mesh> objMeshs;
    [SerializeField] private List<Animal> objAnimals;
    public List<Animal> animals;

    public List<ObjectMaterial> objMaterials;

    public List<Dictionary<int,int>> objColorCounter;

    [SerializeField] private Transform ObjParent;

    [SerializeField] private List<Obj> objs = new List<Obj>();
    public Queue<Tuple<int,int>> splitQueue = new Queue<Tuple<int, int>>();

    [SerializeField] private GameObject splitEffectPrefab;

    [SerializeField] private Queue<ObjData> NextObjs;

    public ObjData HoldObj = new ObjData(ObjType.None,-1);

    [SerializeField] private List<IntangibleObj> intangibleObjs = new List<IntangibleObj>(); // 0:Hold, 1:Next1, 2:Next2, 3:Next3, 4:Next4, 5:Next5

    [SerializeField] public Material BombMaterial;

    [SerializeField] private Bounds[] wallAndFloorBounds = new Bounds[5]; // 0:Floor, 1:LeftWall, 2:RightWall, 3:FrontWall, 4:BackWall

    public List<Collider> placedObjColliders = new List<Collider>();


    //先に定義しておく定数
    private int splitOffset = 20;

    [SerializeField] private float timeSinceLastBomb = 1000f;

    
    private void FixedUpdate()
    {
        if (GameManager.instance.isCleared)
        {
            
        }
        else
        {
            if(GameManager.instance.sceneManager.activeSceneName == "Game"){
                if(NextObjs != null && NextObjs.Count < 5){
                    int rand = Random.Range(0, 100);
                    if(timeSinceLastBomb >= 10f && rand < 10)
                    {
                        NextObjs.Enqueue(new ObjData(ObjType.Bomb,-1));
                        timeSinceLastBomb = 0f;
                    }
                    else{
                        int color = -1,type = Random.Range(0, GameManager.instance.typeCount);
                        
                        if(type == (int)ObjType.Animals) type++;
                        if(type == (int)ObjType.Geometric) type++;
                        if (GameManager.instance.hasAdaptiveColor)
                        {
                            int typeind = type;
                            //if(typeind >= (int)ObjType.Animals) typeind--;
                            //if(typeind >= (int)ObjType.Geometric) typeind--;
                            int minCount = int.MaxValue;
                            List<int> minColors = new List<int>();
                            for(int i = 2;i < GameManager.instance.colorCount+2; i++)
                            {
                                //Debug.Log((int)type + "," + i);
                                //Debug.Log(i + ": " + GameManager.instance.objManager.objColorCounter[(int)type][i] + " " + minCount + " " + string.Join(", ", minColors.ToArray()));
                                //Debug.Log(typeind+" "+objColorCounter.Count);
                                if(objColorCounter[typeind][i] < minCount)
                                {
                                    minCount = objColorCounter[typeind][i];
                                    minColors.Clear();
                                    minColors.Add(i);
                                }
                                else if(objColorCounter[typeind][i] == minCount)
                                {
                                    minColors.Add(i);
                                }
                            }
                            color = minColors[Random.Range(0, minColors.Count)];
                        }
                        else color = Random.Range(2,GameManager.instance.colorCount+2);
                        if(GameManager.instance.isOnlyAnimals) type += (int)ObjType.Geometric+1;
                        NextObjs.Enqueue(new ObjData((ObjType)type, color));
                        Debug.Log("Enqueue NextObj : "+(ObjType)type+" "+color);
                    }
                }
                timeSinceLastBomb += Time.fixedDeltaTime;

                for(int i = 0;i < ObjClearLineTouchingTimes.Count; i++)
                {
                    if(ControllingObj != null && i == ControllingObj.id) continue;
                    if(ObjClearLineTouchingTimes[i] >= 0) ObjClearLineTouchingTimes[i]++;
                    if(ObjClearLineTouchingTimes[i] >= 180){
                        GameManager.instance.ClearGame();
                        Debug.Log(i+" "+ objs[i].type+" "+objs[i].color);
                        break;
                    }
                }

                if(splitQueue.Count > 0 && GameManager.instance.timeSinceLastSplit > splitQueue.Count*0.3f)
                {
                    var pair = splitQueue.Dequeue();
                    //Debug.Log("Split Queue: "+pair.Item1+" "+pair.Item2);
                    if(objs[pair.Item1] != null && objs[pair.Item2] != null){
                        var first = objs[pair.Item1];
                        var second = objs[pair.Item2];
                        if(first != null && second != null) StartCoroutine(SplitObjCoroutine((first.transform.position+second.transform.position)/2, first, second));
                        //Debug.Log("Split Queue Count: "+splitQueue.Count);
                    }
                }

                intangibleObjs[0].SetObjData(HoldObj);
                var nextArray = NextObjs.ToArray();
                for(int i = 0;i < 5; i++)
                {
                    if(nextArray.Length <= i) intangibleObjs[i+1].SetObjData(new ObjData(ObjType.None, -1));
                    else{
                        intangibleObjs[i+1].SetObjData(nextArray[i]);
                        //Debug.Log("NextObj "+i+": "+nextArray[i].type+" "+nextArray[i].color);
                    }
                }
            }
        }
    }

    public void StartGame()
    {
        ObjClearLineTouchingTimes = new List<int>();
        ObjClearLineTouchingColliderCounts = new List<int>();
        objs = new List<Obj>();
        NextObjs = new Queue<ObjData>();
        isObjMoving = false;
        ControllingObj = null;
        countAfterSplit = 4;
        if(GameManager.instance.hasRelativeSplitCount) countAfterSplit = Mathf.Max(3,GameManager.instance.colorCount-1);
        ObjParent = GameObject.Find("Objects' Parent").transform;
        splitQueue = new Queue<System.Tuple<int, int>>();
        intangibleObjs[0] = GameObject.Find("HoldObj").GetComponent<IntangibleObj>();
        for(int i = 1;i <= 5; i++)
        {
            //Debug.Log("Find: "+"NextObj"+i);
            intangibleObjs[i] = GameObject.Find("NextObj"+i).GetComponent<IntangibleObj>();
        }
        List<Transform> fields = GameObject.Find("Fields").GetComponentsInChildren<Transform>().ToList();
        //Debug.Log("Find: "+fields.Count);
        for(int i = 1;i <= 5; i++)
        {   
            //Debug.Log(fields[i].name);
            wallAndFloorBounds[i-1] = fields[i].GetComponent<Collider>().bounds;
        }

        GenerateMaterials();
        SetAnimals();

        /*
        float margin = 12f;
        for(int i = 0;i < GameManager.instance.colorCount*GameManager.instance.typeCount/2; i++)
        {
            GenerateObject(new Vector3(Random.Range(-GameManager.instance.STAGE_WIDTH+margin,GameManager.instance.STAGE_WIDTH-margin),GameManager.instance.STAGE_HEIGHT,Random.Range(-GameManager.instance.STAGE_WIDTH+margin,GameManager.instance.STAGE_WIDTH-margin)),0);
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
        if(GameManager.instance.isOnlyAnimals) animals.RemoveAll(a => a.type < ObjType.Geometric+1);
        else animals.RemoveAll(a => a.type <= ObjType.Animals && a.type >= ObjType.Geometric);
        objColorCounter = new List<Dictionary<int, int>>(animals.Count);
        for(int i = 0;i < Mathf.Min(animals.Count, GameManager.instance.typeCount); i++)
        {
            objColorCounter.Add(new Dictionary<int, int>());
            for(int j = 3;j < Mathf.Min(objMaterials.Count, GameManager.instance.colorCount + 3); j++)
            {
                objColorCounter[i].Add(j-1, 0);
            }
        }
    }

    public void ObjectTouchClearLine(int id)
    {
        //Debug.Log("Enter:"+id);
        if(id == ObjClearLineTouchingTimes.Count && isObjMoving) return;
        if(ControllingObj != null && id == ControllingObj.id) return;
        ObjClearLineTouchingColliderCounts[id]++;
        if(ObjClearLineTouchingColliderCounts[id] > 0 && ObjClearLineTouchingTimes[id] < 0) ObjClearLineTouchingTimes[id] = 0;
    }

    public void ObjectUntouchClearLine(int id)
    {
        //Debug.Log("Exit:"+id);
        //if(id == ObjClearLineTouchingTimes.Count && isObjMoving) return;
        ObjClearLineTouchingColliderCounts[id]--;
        if(ObjClearLineTouchingColliderCounts[id] == 0) ObjClearLineTouchingTimes[id] = -1;
    }

    public void NextObj()
    {
        //Debug.Log("NextObj");
        if(GameManager.instance.isCleared || ControllingObj) return;
        if(objMaterials.Count == 0 || animals.Count == 0) return;
        if(NextObjs.Count == 0) return;
        isObjMoving = true;
        var data = NextObjs.Dequeue();
        int shape = (int)data.type;
        int color = data.color;
        //Debug.Log("NextObj : "+shape+" "+color);
        Obj obj = GenerateObject(CalcPositionUtils.GetRandomPosInStage(),0,shape,color);
        ControllingObj = obj;
        ControllingObj.Predict();
        GameManager.instance.movecount++;
        GameManager.instance.inputManager.timeSinceLastHold = 0f;
    }

    public void DropObj()
    {
        if(ControllingObj == null || GameManager.instance.isCleared || ControllingObj.isTouchingOutside) return;
        ControllingObj.Drop();
        foreach(var col in ControllingObj.col)
        {
            placedObjColliders.Add(col);
        }
        ControllingObj = null;
        isObjMoving = false;
        if(GameManager.instance.predictor != null) GameManager.instance.predictor.RemovePredict();
    }

    public void AddSplitObj(int firstId, int secondId)
    {
        if(GameManager.instance.isCleared || ObjClearLineTouchingTimes.Count > GameManager.instance.MAX_OBJECT_COUNT) return;
        splitQueue.Enqueue(new Tuple<int, int>(firstId, secondId));
    }

    public System.Collections.IEnumerator SplitObjCoroutine(Vector3 pos, Obj first, Obj second)
    {
        if(GameManager.instance.isCleared || ObjClearLineTouchingTimes.Count > GameManager.instance.MAX_OBJECT_COUNT) yield break;
        GameManager.instance.comboCount++;
        GameManager.instance.scoreManager.AddScore(GameManager.instance.comboCount);
        GameManager.instance.timeSinceLastSplit = 0f;
        //Debug.Log("Split"+first.id+" "+second.id);
        ObjClearLineTouchingColliderCounts[first.id] = 0;
        ObjClearLineTouchingColliderCounts[second.id] = 0;
        //*
        ObjClearLineTouchingTimes[first.id] = -1;
        ObjClearLineTouchingTimes[second.id] = -1;
        //*/
        //Debug.Log("first:"+first.id+" second:"+second.id);
        Destroy(first.gameObject);
        Destroy(second.gameObject);
        if(first == ControllingObj || second == ControllingObj) isObjMoving = false;
        for(int i = 0;i < Random.Range(3, 5); i++)
        {
            Instantiate(splitEffectPrefab, pos, Quaternion.identity).GetComponent<SplitEffect>().SetTarget(new Vector3(pos.x, 50, pos.z));
        }
        yield return new WaitForSeconds(0.5f);
        SplitObj(pos, first, second);
    }
    
    public void SplitObj(Vector3 pos, Obj first, Obj second)
    {
        int typeind = (int)first.type;
        if(typeind >= (int)ObjType.Animals) typeind--;
        if(typeind >= (int)ObjType.Geometric) typeind--;
        if(GameManager.instance.isOnlyAnimals) typeind -= (int)ObjType.Geometric;
        int nextType = (int)animals[typeind].next;
        int newSplitCount = Mathf.Max(second.splitCount,first.splitCount) + 1;
        //Debug.Log(typeind);
        //Debug.Log("SplitObj : From"+first.type+" To"+(ObjType)nextType+" "+newSplitCount);

        for(int i = 0;i < countAfterSplit; i++)
        {
            float theta = 2.0f * Mathf.PI * i / countAfterSplit;

            float x = splitOffset * Mathf.Cos(theta);
            float z = splitOffset * Mathf.Sin(theta);

            Vector3 p = new Vector3(pos.x + x, pos.y, pos.z + z);

            p = CalcPositionUtils.CalcValidPos(p, animals[typeind].prefab.transform.localScale);

            var obj = GenerateObject(p, first.splitCount+1, nextType);
            obj.state = Obj.State.Finished;
            obj.ChangeLayerToPlaced();
            obj.rb.constraints = RigidbodyConstraints.None;
            if(ObjClearLineTouchingTimes.Count >= GameManager.instance.MAX_OBJECT_COUNT) return;
        }
    }

    public Obj GenerateObject(Vector3 pos, int splitCount, int type = -1, int color = -3)
    {
        if (type == -1) type = Random.Range(0, GameManager.instance.typeCount);
        if (color == -3) color = Random.Range(2,GameManager.instance.colorCount+2);
        //Debug.Log("GenerateObject : "+(ObjType)type+" "+color);
        //Debug.Log(type+" "+animals.Count+" "+GameManager.instance.isOnlyAnimals);
        int typeind = type;
        if(type >= (int)ObjType.Animals) typeind--;
        if(type >= (int)ObjType.Geometric) typeind--;
        if(GameManager.instance.isOnlyAnimals) typeind -= (int)ObjType.Geometric;
        if(type != (int)ObjType.Bomb && typeind >= GameManager.instance.typeCount) typeind = 0;
        if(type == (int)ObjType.Bomb){
            typeind = animals.Count-1;
            color = -1;
        }
        var obj = Instantiate(animals[typeind].prefab, new Vector3(0,100,0), Quaternion.identity, ObjParent);
        /*
        float loc = Mathf.Pow(1.2f, level);
        Vector3 def = obj.transform.localScale;
        Vector3 scale = new Vector3(def.x*loc, def.y*loc, def.z*loc);
        obj.transform.localScale = scale;
        //*/
        Vector3 scale = obj.transform.localScale;
        //*
        pos.y = GameManager.instance.STAGE_HEIGHT;
        //*/
        pos = CalcPositionUtils.CalcValidPos(pos,scale);
        obj.transform.position = pos;
        var objObj = obj.GetComponent<Obj>();
        objObj.type = (ObjType)type;
        objObj.splitCount = splitCount;
        if(color != -1){
            objObj.color = color;
            objColorCounter[typeind][color]++;
        }
        if(type != (int)ObjType.Bomb){
            objs.Add(objObj);
            
        }
        objObj.Init();

        var aabb = CollisionUtility.CreateAABB(obj.GetComponentsInChildren<Collider>().ToList());

        if(CollisionUtility.IsCollidingWithWallOrFloor(aabb,wallAndFloorBounds.ToList()))
        {
            Destroy(obj);
            return GenerateObject(CalcPositionUtils.GetRandomPosInStage(), splitCount, type, color);
        }
        objObj.rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        return objObj;
    }

    

    public void ExplodeBomb(Vector3 pos)
    {
        foreach (var obj in objs)
        {
            if(obj == null) continue;
            Vector3 objpos = obj.transform.position;
            Vector3 dir = objpos - pos;
            dir = dir.normalized;
            obj.rb.AddForce(dir*obj.rb.mass*50, ForceMode.Impulse);
        }
    }
}
