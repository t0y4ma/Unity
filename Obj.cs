using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Obj : MonoBehaviour
{
    public enum State
    {
        None,
        StandBy,
        Moving,
        Finished
        
    }
    public State state = State.StandBy;
    private float stoppingTime = 0f;
    public Vector3 prevPos = new Vector3(-100,-100,-100);
    public Vector3 destination = new Vector3(0,-100,0);
    public Rigidbody rb;
    public MeshRenderer mr;
    public List<Collider> col;
    public int id = -1;
    public int color = -1;
    public ObjType type = ObjType.None;
    public int splitCount = 0;

    private bool isCollidedEver = false;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        mr = gameObject.GetComponent<MeshRenderer>();
        var c = gameObject.GetComponent<Collider>();
        if(c != null)
        {
            col.Add(c);
        }
        else
        {
            col = gameObject.GetComponentsInChildren<Collider>().ToList();
        }
        gameObject.layer = LayerMask.NameToLayer("Controling Objects");
    }

    void Start()
    {
        //Debug.Log(GameManager.instance == null);
        id = GameManager.instance.objManager.ObjClearLineTouchingTimes.Count;
        if (GameManager.instance.hasAdaptiveColor)
        {
            int minCount = int.MaxValue;
            List<int> minColors = new List<int>();
            for(int i = 2;i < GameManager.instance.objManager.objMaterials.Count; i++)
            {
                //Debug.Log(GameManager.instance.objManager.objColorCounter[(int)type][i] + " " + minCount + " " + minColor);
                if(GameManager.instance.objManager.objColorCounter[(int)type][i] < minCount)
                {
                    minCount = GameManager.instance.objManager.objColorCounter[(int)type][i];
                    minColors.Clear();
                    minColors.Add(i);
                }
                else if(GameManager.instance.objManager.objColorCounter[(int)type][i] == minCount)
                {
                    minColors.Add(i);
                }
            }
            color = minColors[Random.Range(0, minColors.Count)];
        }
        else color = Random.Range(2,GameManager.instance.objManager.objMaterials.Count);
        GameManager.instance.objManager.objColorCounter[(int)type][color]++;
        GameManager.instance.objManager.ObjClearLineTouchingTimes.Add(-1);
        mr.material = GameManager.instance.objManager.objMaterials[color].material;
        if(GameManager.instance.objManager.ControllingObj == this){
            //Debug.Log("Being Controled:"+id);
            foreach(Collider c in col)
            {
                c.enabled = false;
            }
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Placed Objects");
        }
        //Debug.Log("id:"+id);
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    void FixedUpdate()
    {
        //Debug.Log(rb.linearVelocity);
        if (Mathf.Abs(transform.position.y) > GameManager.instance.STAGE_HEIGHT*2) {
            //Debug.Log("id:"+id);
            GameManager.instance.objManager.ObjectUntouchClearLine(id);
            Destroy(gameObject);
        }
        if(Mathf.Abs(transform.position.x) > GameManager.instance.STAGE_WIDTH || Mathf.Abs(transform.position.z) > GameManager.instance.STAGE_WIDTH)
        {
            GameManager.instance.objManager.ObjectUntouchClearLine(id);
        }
        if(GameManager.instance.objManager.ControllingObj == this){
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
        else{
            rb.useGravity = true;
        }

        switch (state)
        {
            case State.StandBy:
                break;
            case State.Moving:
                float dif = Mathf.Abs(transform.position.x - prevPos.x) + Mathf.Abs(transform.position.y - prevPos.y) + Mathf.Abs(transform.position.z - prevPos.z);
                //Debug.Log(dif);
                //Debug.Log(prevPos);
                //Debug.Log(transform.position);
                if (dif < 0.01f) stoppingTime += Time.deltaTime;
                else stoppingTime = 0;
                if (stoppingTime > 0.25f)
                {
                    state = State.Finished;
                    if(GameManager.instance.objManager.ControllingObj == this) GameManager.instance.objManager.isObjMoving = false;
                }
                break;
            case State.Finished:
                break;
            default:
                break;
        }
        prevPos = transform.position;

        if(isCollidedEver) rb.linearVelocity = new Vector3(rb.linearVelocity.x*0.96f,rb.linearVelocity.y,rb.linearVelocity.z*0.96f);
        if(destination.y != -100)
        {
            var dif = destination-transform.position;
            if(dif.sqrMagnitude < 0.025){
                transform.position = destination;
                //Debug.Log(transform.eulerAngles);
                Predict();
                destination.y = -100;
            }
            else transform.position = transform.position+dif/2;
        }
        updateMaterial();
        Debug.DrawLine(transform.position,transform.position+transform.up*-10,Color.blue);
    }
    
    public void Predict()
    {
        if(GameManager.instance.objManager.ControllingObj != this) return;
        if(GameManager.instance.predictor == null) return;
        GameManager.instance.predictor.RemovePredict();
        GameManager.instance.predictor.Predict(transform.position,Vector3.down,GameManager.instance.DROP_FIRSTSPEED);
    }

    private void updateMaterial()
    {
        /*
        if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.objMaterials[0]) mr.material = GameManager.instance.objManager.objMaterials[0]; }
        else if(mr.material != GameManager.instance.objManager.objMaterials[color]) mr.material = GameManager.instance.objManager.objMaterials[color];
        //*/

        if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.objMaterials[0].material) mr.material = GameManager.instance.objManager.objMaterials[0].material; }
        else if(mr.material != GameManager.instance.objManager.objMaterials[color].material) mr.material = GameManager.instance.objManager.objMaterials[color].material;

        /*
        if(type == -1)
        {
            if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.objMaterials[0]) mr.material = GameManager.instance.objManager.objMaterials[0]; }
            else if(mr.material != GameManager.instance.objManager.objMaterials[color]) mr.material = GameManager.instance.objManager.objMaterials[color];
        }
        else
        {
            if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.animals[level][type].materials[0]) mr.material = GameManager.instance.objManager.animals[level][type].materials[0]; }
            else if(mr.material != GameManager.instance.objManager.animals[level][type].materials[color]) mr.material = GameManager.instance.objManager.animals[level][type].materials[color];
        }
        //*/
    }
    
    public void Drop()
    {
        gameObject.layer = LayerMask.NameToLayer("Placed Objects");
        prevPos = new Vector3(-100, -100, -100);
        destination = new Vector3(0, -100, 0);
        state = State.Moving;
        rb.AddForce(Vector3.down*rb.mass*GameManager.instance.DROP_FIRSTSPEED, ForceMode.Impulse);
        foreach(Collider c in col)
        {
            c.enabled = true;
        }
        GameManager.instance.timeSinceLastDrop = 0f;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        isCollidedEver = true;
        //return;
        Obj obj = collision.gameObject.GetComponent<Obj>();
        if (obj == null || obj.id > id || GameManager.instance.objManager.ControllingObj == obj || GameManager.instance.objManager.ControllingObj == this) return;
        if (obj.splitCount >= GameManager.instance.MAXSPLITCOUNT || splitCount >= GameManager.instance.MAXSPLITCOUNT) return;
        if(GameManager.instance.objManager.animals[(int)type].next == ObjType.None) return;
        if (obj.color == color && obj.type == type)
        {
            GameManager.instance.objManager.SplitObject(transform.position, this, obj);
        }
    }

    private void OnApplicationQuit()
    {
        Physics.simulationMode = SimulationMode.Script;
    }

    private void OnSceneChange(Scene previousScene, Scene newScene)
    {
        Physics.simulationMode = SimulationMode.Script;
    }
}
