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
    public Collider col;
    public int id;
    public int color;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        mr = gameObject.GetComponent<MeshRenderer>();
        col = gameObject.GetComponent<Collider>();
        gameObject.layer = LayerMask.NameToLayer("Controling Objects");
        mr.material = GameManager.instance.objControlManager.ObjMaterials[color];
        if(GameManager.instance.objControlManager.ControllingObj == this){
            //Debug.Log("Being Controled:"+id);
            col.enabled = false;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Placed Objects");
        }
        //Debug.Log("id:"+id);
    }

    void FixedUpdate()
    {
        if (Mathf.Abs(transform.position.y) > 10) {
            Debug.Log("id:"+id);
            GameManager.instance.objControlManager.ObjectUntouchClearLine(id);
            Destroy(gameObject);
        }
        if(Mathf.Abs(transform.position.x) > GameManager.instance.stagesize || Mathf.Abs(transform.position.x) > GameManager.instance.stagesize)
        {
            GameManager.instance.objControlManager.ObjectUntouchClearLine(id);
        }
        if(GameManager.instance.objControlManager.ControllingObj == this){
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
                if (stoppingTime > 0.5f)
                {
                    state = State.Finished;
                    StartCoroutine(GameManager.instance.objControlManager.NextObj());
                }
                break;
            case State.Finished:
                break;
            default:
                break;
        }
        //Debug.Log(stoppingTime);
        //Debug.Log(prevPos);
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
        if(GameManager.instance.objControlManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objControlManager.ObjMaterials[0]) mr.material = GameManager.instance.objControlManager.ObjMaterials[0]; }
        else if(mr.material != GameManager.instance.objControlManager.ObjMaterials[color]) mr.material = GameManager.instance.objControlManager.ObjMaterials[color];
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        //return;
        Obj obj = collision.gameObject.GetComponent<Obj>();
        if (obj == null || obj.id > id || GameManager.instance.objControlManager.ControllingObj == obj || GameManager.instance.objControlManager.ControllingObj == this) return;
        if (level == 0) return;
        if (obj.level == level && obj.color == color)
        {
            GameManager.instance.objControlManager.SplitObject(level - 1, transform.position, this, obj);
        }
    }
}
