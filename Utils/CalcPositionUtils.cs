using UnityEngine;

public static class CalcPositionUtils
{
    public static Vector3 GetRandomPosInStage()
    {
        return new Vector3(Random.Range(0,GameManager.instance.STAGE_WIDTH*9)/10*(-1+Random.Range(0,2)),GameManager.instance.STAGE_HEIGHT,Random.Range(0,GameManager.instance.STAGE_WIDTH*9)/10*(-1+Random.Range(0,2)));
    }
    
    public static Vector3 CalcValidPos(Vector3 pos,Vector3 scale)
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
