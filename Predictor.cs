using System.Collections.Generic;
using UnityEngine;

public class Predictor : MonoBehaviour
{
    [SerializeField] private GameObject predictSignPrefab;
    List<GameObject> predictSigns = new List<GameObject>();

    public void Start()
    {
        predictSignPrefab = Resources.Load("Prefabs/Predict Sign") as GameObject;
    }
    
    public void RemovePredict()
    {
        foreach (GameObject obj in predictSigns)
        {
            Destroy(obj);
        }
        predictSigns.Clear();
    }

    public void Predict(Vector3 startPos, Vector3 forward, float speed)
{
    Vector3 direction = forward;
    Vector3 velocity = direction * speed;

    float timeStep = 0.1f;
    float maxTime = 5f;

    Vector3 prevPos = startPos;

    bool drawflg = false;

    for (float t = 0; t < maxTime; t += timeStep)
    {
        Vector3 pos = startPos 
                    + velocity * t 
                    + 0.5f * Physics.gravity * t * t;

        Debug.DrawLine(prevPos, pos, Color.red, 5);

        RaycastHit hit;
        if (Physics.Raycast(prevPos, pos - prevPos, out hit, (pos - prevPos).magnitude))
        {
            //Debug.Log("Hit " + hit.collider.name);
            break;
        }

        if((pos - startPos).sqrMagnitude > 10f) drawflg = true;

        if(drawflg)
        {
            predictSigns.Add(
                Instantiate(predictSignPrefab, pos, Quaternion.identity)
            );
        }

        prevPos = pos;
    }
}

    public static Vector3 GetLaunchDirection(float xAngle, float yAngle)
    {
        Quaternion rot = Quaternion.Euler(xAngle, yAngle, 0f);
        return rot * Vector3.forward;
    }
}   
