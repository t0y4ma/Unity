using UnityEngine;

public class Bomb : Obj
{

    [SerializeField] private int spawnTiming;

    override public void Init()
    {
        type = ObjType.Bomb;
        spawnTiming = GameManager.instance.movecount;
    }

    override protected void FixedUpdate()
    {
        base.FixedUpdate();
        if(GameManager.instance.movecount > spawnTiming + GameManager.instance.COUNT_UNTIL_BOMB_EXPLODE)
        {
            Invoke("Explode", 0.5f);
        }
    }

    private void Explode()
    {
        GameManager.instance.objManager.ExplodeBomb(transform.position);
        Destroy(gameObject);
    }

    override protected void OnCollisionEnter(Collision collision)
    {
        isCollidedEver = true;
        return;
    }

    override protected void updateMaterial()
    {
        return;
    }
}