using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Unity.IntegerTime;
using UnityEngine;

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
    public int level = 0;
    private float stoppingTime = 0f;
    public Vector3 prevPos = new Vector3(-100,-100,-100);
    public Vector3 destination = new Vector3(0,-100,0);
    public Rigidbody rb;
    public MeshRenderer mr;
    public List<Collider> col;
    public int id = -1;
    public int color = -1;
    public int type = -1;

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
        Debug.Log(GameManager.instance == null);
        id = GameManager.instance.objManager.ObjClearLineTouchingTimes.Count;
        color = Random.Range(1,GameManager.instance.objManager.objAnimals[level].materials.Count);
        GameManager.instance.objManager.ObjClearLineTouchingTimes.Add(-1);
        mr.material = GameManager.instance.objManager.objAnimals[level].materials[color];
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
    }

    void FixedUpdate()
    {
        //Debug.Log(rb.linearVelocity);
        if (Mathf.Abs(transform.position.y) > GameManager.instance.stageHeight*2) {
            Debug.Log("id:"+id);
            GameManager.instance.objManager.ObjectUntouchClearLine(id);
            Destroy(gameObject);
        }
        if(Mathf.Abs(transform.position.x) > GameManager.instance.stagesize || Mathf.Abs(transform.position.z) > GameManager.instance.stagesize)
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
                    GameManager.instance.objManager.isObjMoving = false;
                }
                break;
            case State.Finished:
                break;
            default:
                break;
        }
        prevPos = transform.position;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x*0.96f,rb.linearVelocity.y,rb.linearVelocity.z*0.96f);
        if(destination.y != -100)
        {
            var dif = destination-transform.position;
            if(dif.sqrMagnitude < 0.025){
                transform.position = destination;
                Debug.Log(transform.eulerAngles);
                GameManager.instance.predictor.Predict(transform.position,transform.eulerAngles+new Vector3(90,0,0));
                destination.y = -100;
            }
            else transform.position = transform.position+dif/2;
        }
        updateMaterial();
    }

    private void updateMaterial()
    {
        if(type == -1)
        {
            if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.objAnimals[level].materials[0]) mr.material = GameManager.instance.objManager.objAnimals[level].materials[0]; }
            else if(mr.material != GameManager.instance.objManager.objAnimals[level].materials[color]) mr.material = GameManager.instance.objManager.objAnimals[level].materials[color];
        }
        else
        {
            if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.animals[level][type].materials[0]) mr.material = GameManager.instance.objManager.animals[level][type].materials[0]; }
            else if(mr.material != GameManager.instance.objManager.animals[level][type].materials[color]) mr.material = GameManager.instance.objManager.animals[level][type].materials[color];
        }
    }
    
    public void Drop()
    {
        gameObject.layer = LayerMask.NameToLayer("Placed Objects");
        prevPos = new Vector3(-100, -100, -100);
        destination = new Vector3(0, -100, 0);
        state = Obj.State.Moving;
        rb.AddForce(Vector3.down*1000, ForceMode.Impulse);
        foreach(Collider c in col)
        {
            c.enabled = true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //return;
        Obj obj = collision.gameObject.GetComponent<Obj>();
        if (obj == null || obj.id > id || GameManager.instance.objManager.ControllingObj == obj || GameManager.instance.objManager.ControllingObj == this) return;
        if (level == 0) return;
        if (obj.level == level && obj.color == color)
        {
            GameManager.instance.objManager.SplitObject(level - 1, transform.position, this, obj);
        }
    }
}
