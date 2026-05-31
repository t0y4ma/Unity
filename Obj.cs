using System.Collections.Generic;
using System.Linq;
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
    private float stoppingTime = 0f;
    public Vector3 prevPos = new Vector3(-100,-100,-100);
    public Vector3 destination = new Vector3(0,-100,0);
    public Rigidbody rb;
    public MeshRenderer mr;
    public List<Collider> col = new List<Collider>();
    public int id = -1;
    public int color = -1;
    public ObjType type = ObjType.None;
    public int splitCount = 0;
    protected bool isCollidedEver = false;
    public bool isSplited = false;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        mr = gameObject.GetComponentInChildren<MeshRenderer>();
        var c = gameObject.GetComponent<Collider>();
        if(c != null)
        {
            col.Add(c);
        }
        else
        {
            col = gameObject.GetComponentsInChildren<Collider>().ToList();
        }
        foreach(var collider in col)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("Controling Objects");
        }
    }

    public virtual void Init()
    {
        //Debug.Log(GameManager.instance == null);
        id = GameManager.instance.objManager.ObjClearLineTouchingTimes.Count;
        /*
        if (GameManager.instance.hasAdaptiveColor)
        {
            int minCount = int.MaxValue;
            List<int> minColors = new List<int>();
            for(int i = 2;i < GameManager.instance.colorCount+2; i++)
            {
                //Debug.Log((int)type + "," + i);
                //Debug.Log(i + ": " + GameManager.instance.objManager.objColorCounter[(int)type][i] + " " + minCount + " " + string.Join(", ", minColors.ToArray()));
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
        //*/
        GameManager.instance.objManager.ObjClearLineTouchingTimes.Add(-1);
        GameManager.instance.objManager.ObjClearLineTouchingColliderCounts.Add(0);
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
            foreach(var collider in col)
            {
                collider.gameObject.layer = LayerMask.NameToLayer("Controling Objects");
            }
        }
    }

    protected virtual void FixedUpdate()
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
        /*
        if(GameManager.instance.objManager.splitQueue.Count > 0)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
        }
        //*/

        if(GameManager.instance.objManager.ControllingObj == this){
            //Debug.Log(gameObject.name+" "+id);
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
        else{
            rb.useGravity = true;
            if(rb.linearVelocity == Vector3.zero)
            {
                //Debug.Log(id);
            }
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

    protected virtual void updateMaterial()
    {
        if(color == -1 || !isCollidedEver) return;

        if(color != 1 && splitCount == GameManager.instance.MAX_SPLIT_COUNT){
            int typeind = (int)type;
            if(typeind >= (int)ObjType.Animals) typeind--;
            if(typeind >= (int)ObjType.Geometric) typeind--;
            if(GameManager.instance.isOnlyAnimals) typeind -= (int)ObjType.Geometric;
            GameManager.instance.objManager.objColorCounter[typeind][color]--;
            color = 1;
        }

        if(GameManager.instance.objManager.ObjClearLineTouchingTimes[id] >= 0 && state != State.StandBy){ if(mr.material != GameManager.instance.objManager.objMaterials[0].material) mr.material = GameManager.instance.objManager.objMaterials[0].material; }
        else if(mr.material != GameManager.instance.objManager.objMaterials[color].material) mr.material = GameManager.instance.objManager.objMaterials[color].material;
    }
    
    public void Drop()
    {
        ChangeLayerToPlaced();
        prevPos = new Vector3(-100, -100, -100);
        destination = new Vector3(0, -100, 0);
        state = State.Moving;
        rb.AddForce(Vector3.down*rb.mass*GameManager.instance.DROP_FIRSTSPEED, ForceMode.Impulse);
        foreach(Collider c in col)
        {
            c.enabled = true;
        }
        GameManager.instance.timeSinceLastDrop = 0f;
        rb.constraints = RigidbodyConstraints.None;
    }
    
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Default")) return;
        isCollidedEver = true;
        ChangeLayerToPlaced();
        //return;
        if(GameManager.instance.objManager.ObjClearLineTouchingTimes.Count >= GameManager.instance.MAX_OBJECT_COUNT) return;
        Obj obj = collision.gameObject.GetComponent<Obj>();
        if (obj == null || obj.id > id || GameManager.instance.objManager.ControllingObj == obj || GameManager.instance.objManager.ControllingObj == this) return;
        if(obj.isSplited || isSplited) return;
        if (obj.splitCount >= GameManager.instance.MAX_SPLIT_COUNT || splitCount >= GameManager.instance.MAX_SPLIT_COUNT) return;
        int typeind = (int)type;
        if(typeind >= (int)ObjType.Animals) typeind--;
        if (typeind >= (int)ObjType.Geometric) typeind--;
        if(GameManager.instance.isOnlyAnimals) typeind -= (int)ObjType.Geometric;
        if(GameManager.instance.objManager.animals[typeind].next == ObjType.None) return;
        if(id == -1 || obj.id == -1) return;
        if (obj.color == color && obj.type == type)
        {
            isSplited = true;
            obj.isSplited = true;
            GameManager.instance.objManager.AddSplitObj(id, obj.id);
        }
    }

    public void ChangeLayerToPlaced()
    {
        foreach(var collider in col)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("Placed Objects");
        }
    }

    public void RotateRight()
    {
        transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, Vector3.up, -GameManager.instance.ROTATE_SPEED);
        foreach(var c in col) if(CollisionUtility.IsColliding(c, GameManager.instance.objManager.placedObjColliders))
        {
            transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, Vector3.up, GameManager.instance.ROTATE_SPEED);
            break;
        }
        Predict();
    }
    public void RotateLeft()
    {
        transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, Vector3.up, GameManager.instance.ROTATE_SPEED);
        foreach(var c in col) if(CollisionUtility.IsColliding(c, GameManager.instance.objManager.placedObjColliders))
        {
            transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, Vector3.up, -GameManager.instance.ROTATE_SPEED);
            break;
        }
        Predict();
    }

    public void RotateFront()
    {
        transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, GameManager.instance.objManager.ControllingObj.transform.right, -GameManager.instance.ROTATE_SPEED);
        foreach(var c in col) if(CollisionUtility.IsColliding(c, GameManager.instance.objManager.placedObjColliders))
        {
            transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, GameManager.instance.objManager.ControllingObj.transform.right, GameManager.instance.ROTATE_SPEED);
            break;
        }
        Predict();
    }

    public void RotateBack()
    {
        transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, GameManager.instance.objManager.ControllingObj.transform.right, GameManager.instance.ROTATE_SPEED);
        foreach(var c in col) if(CollisionUtility.IsColliding(c, GameManager.instance.objManager.placedObjColliders))
        {
            transform.RotateAround(GameManager.instance.objManager.ControllingObj.transform.position, GameManager.instance.objManager.ControllingObj.transform.right, -GameManager.instance.ROTATE_SPEED);
            break;
        }
        Predict();
    }


}
