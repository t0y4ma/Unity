using UnityEngine;
using UnityEngine.UI;

public class ButtonAreaImage : MonoBehaviour
{
    public string propName;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private bool isOn = false;

    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        GameManager.instance.inputManager.props[propName] = false;
    }

    public void FixedUpdate()
    {
        if(isOn != GameManager.instance.inputManager.props[propName])
        {
            isOn = GameManager.instance.inputManager.props[propName];
            img.sprite = isOn ? onSprite : offSprite;
        }
    }
}
