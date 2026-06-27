using System.Collections.Generic;
using UnityEngine;

public class EvolutionRingRenderer : MonoBehaviour,IStartGame
{
    public GameObject intangible;

    public List<LineRenderer> lineRenderers = new List<LineRenderer>();

    [SerializeField] private float d = 5f;
    [SerializeField] private float r = 10f;
    [SerializeField] private int segments = 20;
    [SerializeField] private Material lineMaterial;

    public void StartGame()
    {
        int count = GameManager.instance.objManager.animals.Count-1;
        List<Vector2> pos = new List<Vector2>();
        for(int i = 0;i < count; i++)
        {
            float angle = 360f / count * i;
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(rad) * r;
            float y = Mathf.Cos(rad) * r;
            pos.Add(new Vector2(x,y));

            var intangibleobj = Instantiate(intangible, transform.position+new Vector3(x,y,d), Quaternion.identity,transform);
            ObjData data = new ObjData(ObjType.None,0);
            data.type = GameManager.instance.objManager.animals[i].type;
            data.color = 2+i%GameManager.instance.colorCount;
            Debug.Log("SetObjData : "+data.type+" "+data.color);
            intangibleobj.GetComponent<IntangibleObj>().SetObjData(data);
            intangibleobj.layer = LayerMask.NameToLayer("Evolution Ring");
        }

        for(int i = 0;i < count; i++)
        {
            var lr = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            lr.positionCount = segments;

            for(int j = 0;j < segments; j++)
            {
                float t = (float)j / (segments - 1);
                Vector3 start = transform.position+new Vector3(pos[i].x,pos[i].y,d);
                Vector3 end = transform.position+new Vector3(pos[(i+1)%count].x,pos[(i+1)%count].y,d);
                Vector3 control = transform.position+new Vector3((pos[i].x + pos[(i+1)%count].x)/1.5f, (pos[i].y + pos[(i+1)%count].y)/1.5f, d);
                lr.SetPosition(j, SampleCurve(start, end, control, t));
            }

            lr.sharedMaterial = lineMaterial;
            lr.gameObject.layer = LayerMask.NameToLayer("Evolution Ring");
        }
    }

    //https://developer.oculus.com/blog/teleport-curves-with-the-gear-vr-controller/
     Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float t)
     {
         // Interpolate along line S0: control - start;
         Vector3 Q0 = Vector3.Lerp(start, control, t);
         // Interpolate along line S1: S1 = end - control;
         Vector3 Q1 = Vector3.Lerp(control, end, t);
         // Interpolate along line S2: Q1 - Q0
         Vector3 Q2 = Vector3.Lerp(Q0, Q1, t);
         return Q2; // Q2 is a point on the curve at time t
     }
}
