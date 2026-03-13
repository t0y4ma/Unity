using UnityEngine;
using UnityEngine.UI;

public class ButtonAreaImage : MonoBehaviour
{
    public string propName;

    private bool isOn = false;

    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
    }

    public void FixedUpdate()
    {
        if(isOn != GameManager.instance.inputManager.props[propName])
        {
            isOn = GameManager.instance.inputManager.props[propName];
            img.color = isOn ? new Color(1, 1, 1, 0.5f) : new Color(1, 1, 1, 1);
        }
    }
}
