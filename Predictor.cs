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

    public void Predict(Vector3 Pos, Vector3 eulerAngles)
    {
        var normalizedVec = GetLaunchDirection(eulerAngles.x,eulerAngles.y);
        Debug.Log(eulerAngles+" "+normalizedVec);
        var ang = Vector3.zero;
        RaycastHit hitInfo;
        while (ang.sqrMagnitude <= 8000)
        {
            Ray ray= new Ray(Pos+ang,normalizedVec*10);
            //Debug.DrawRay(Pos+ang,normalizedVec,new Color(0,0,0),5000);
            ang += normalizedVec*10;
            if (Physics.Raycast(ray, out hitInfo, 1))
            {
                //Debug.Log(hitInfo.collider.gameObject.name);
                break;
            }
            predictSigns.Add(Instantiate(predictSignPrefab, Pos+ang, Quaternion.identity));
        }
    }

    public static Vector3 GetLaunchDirection(float xAngle, float yAngle)
    {
        Quaternion rot = Quaternion.Euler(xAngle, yAngle, 0f);
        return rot * Vector3.forward;
    }
    
}   
