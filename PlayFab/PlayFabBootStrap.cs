using UnityEngine;

public class PlayFabBootstrap : MonoBehaviour
{
    public static IPlayFabService Service { get; private set; }

    void Awake()
    {
        if(Service != null) return;
        Service = new PlayFabService();
    }
}