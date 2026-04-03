using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public string activeSceneName;

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("SceneLoadManager OnSceneLoaded");
        activeSceneName = scene.name;
    }

    public void StartGame()
    {
        Debug.ClearDeveloperConsole();
        GameManager.instance.uiManager.SetUIText("GameText", "");
        GameManager.instance.uiManager.SetUIActive(UIType.Button, "RestartButton", false);
        GameManager.instance.uiManager.SetUIActive(UIType.Button, "TitleButton", false);
        GameManager.instance.uiManager.SetUIActive(UIType.Text, "New Record", false);
        GameManager.instance.uiManager.SetUIActive(UIType.Other, "Result", false);
        Debug.Log("SceneManager StartGame");
    }
    
    private void OnApplicationQuit()
    {
        Physics.simulationMode = SimulationMode.Script;
    }
}
