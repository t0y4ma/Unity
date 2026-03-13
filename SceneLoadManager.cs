using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoadManager : MonoBehaviour
{
    public Button restartButton;
    public TextMeshProUGUI gameText;

    public void StartGame()
    {
        restartButton = GameObject.Find("RestartButton").GetComponent<Button>();
        gameText = GameObject.Find("GameText").GetComponent<TextMeshProUGUI>();
        restartButton.onClick.AddListener(RestartGame);
        gameText.SetText("");
        restartButton.gameObject.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
        restartButton.enabled = false;
        Debug.Log("SceneManager StartGame");
    }

    public void RestartGame()
    {
        GameManager.instance.isCleared = false;
        GameManager.instance.objManager.ObjClearLineTouchingTimes.Clear();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
