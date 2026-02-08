using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjControlManager : MonoBehaviour
{
    public bool isPlayerControllingObj;
    public Obj ControllingObj;
    public List<int> ObjClearLineTouchingTimes;
    public Material[] ObjMaterials;
    [SerializeField]
    private List<GameObject> objPrefabs;
    private int[] dx = { 1, 1, -1, -1 }, dz = { 1, -1, 1, -1 };

    void Start()
    {
        ObjClearLineTouchingTimes = new List<int>();
        var objlist = Resources.LoadAll<GameObject>("Prefabs/Objects/").ToList();
        objPrefabs = objlist.OrderBy(x => { return x.GetComponent<Obj>().level; }).ToList();
        Debug.Log(objlist.Count());
        isPlayerControllingObj = true;
        StartCoroutine(NextObj());
    }
    
    void FixedUpdate()
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

    public void ObjectTouchClearLine(int id)
    {
        Debug.Log("Enter:"+id);
        if(id == ObjClearLineTouchingTimes.Count && isPlayerControllingObj) return;
        ObjClearLineTouchingTimes[id] = 0;
    }

    public void ObjectUntouchClearLine(int id)
    {
        Debug.Log("Exit:"+id);
        //if(id == ObjClearLineTouchingTimes.Count && isPlayerControllingObj) return;
        ObjClearLineTouchingTimes[id] = -1;
    }

    public IEnumerator NextObj()
    {
        if(GameManager.instance.isCleared || ControllingObj) yield break;
        Debug.Log("NextObj");
        yield return new WaitForSeconds(0.5f);
        if(GameManager.instance.isCleared || ControllingObj) yield break;
        var obj = GenerateObject(Random.Range(0,3), new Vector3(Random.Range(0,GameManager.instance.stagesize*9)/10*(-1+Random.Range(0,2)),GameManager.instance.stageHeight,Random.Range(0,GameManager.instance.stagesize*9)/10*(-1+Random.Range(0,2))));
        ControllingObj = obj;
        isPlayerControllingObj = true;
        GameManager.instance.predictor.Predict(ControllingObj.transform.position,new Vector3(90,0,0));
    }

    public void DropObj()
    {
        if(ControllingObj == null || GameManager.instance.isCleared) return;
        isPlayerControllingObj = false;
        ControllingObj.gameObject.layer = LayerMask.NameToLayer("Placed Objects");
        ControllingObj.prevPos = new Vector3(-100, -100, -100);
        ControllingObj.destination = new Vector3(0, -100, 0);
        ControllingObj.state = Obj.State.Moving;
        ControllingObj.rb.AddForce(Vector3.down, ForceMode.Impulse);
        ControllingObj.col.enabled = true;
        ControllingObj = null;
        GameManager.instance.predictor.RemovePredict();
        return;
    }
    
    public void SplitObject(int level, Vector3 pos, Obj first, Obj second)
    {
        Debug.Log("Split");
        ObjClearLineTouchingTimes[first.id] = -1;
        ObjClearLineTouchingTimes[second.id] = -1;
        Destroy(first.gameObject);
        Destroy(second.gameObject);
        if(ObjClearLineTouchingTimes.Count == first.id+1) StartCoroutine(NextObj());
        if (objPrefabs == null) return;
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = new Vector3(pos.x + dx[i], pos.y, pos.z + dz[i]);
            GenerateObject(level, p).state = Obj.State.Finished;
        }
    }

    public Obj GenerateObject(int level, Vector3 pos)
    {
        var obj = Instantiate(objPrefabs[level], new Vector3(0,100,0), Quaternion.identity);
        /*
        float loc = Mathf.Pow(1.2f, level);
        Vector3 def = obj.transform.localScale;
        Vector3 scale = new Vector3(def.x*loc, def.y*loc, def.z*loc);
        obj.transform.localScale = scale;
        //*/

        Vector3 scale = obj.transform.localScale;
        if (Mathf.Abs(pos.x) > GameManager.instance.stagesize - scale.x / 2)
        {
            if (pos.x < 0) pos.x = -GameManager.instance.stagesize + scale.x / 2;
            else pos.x = GameManager.instance.stagesize - scale.x / 2;
        }
        if (Mathf.Abs(pos.z) > GameManager.instance.stagesize - scale.z / 2)
        {
            if (pos.z < 0) pos.z = -GameManager.instance.stagesize + scale.z / 2;
            else pos.z = GameManager.instance.stagesize - scale.z / 2;
        }
        obj.transform.position = pos;
        var objObj = obj.GetComponent<Obj>();
        objObj.id = ObjClearLineTouchingTimes.Count;
        objObj.color = Random.Range(1,ObjMaterials.Length);
        ObjClearLineTouchingTimes.Add(-1);
        return objObj;
    }
    
}
