using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

#region Structs
[System.Serializable]
struct NecessaryUIs
{
    public Dictionary<string, UIList> uiNames;
    public List<NecessaryUI> visibleUINames;

    public UIList this[string name]
    {
        get
        {
            if (uiNames.ContainsKey(name))
            {
                return uiNames[name];
            }
            else
            {
                foreach (var ui in visibleUINames)
                {
                    if (ui.sceneName == name)
                    {
                        //Debug.Log(name);
                        uiNames[name] = new UIList { uiNames = ui.uiNames };
                        return uiNames[name];
                    }
                }
                return new UIList { uiNames = new List<string>() };
            }
        }
        set
        {
            uiNames[name] = value;
        }
    }
}

[System.Serializable]
struct NecessaryUI
{
    public string sceneName;
    public List<string> uiNames;
}

[System.Serializable]
struct UIList
{
    public List<string> uiNames;
}
#endregion

public enum UIType
{
    Button,
    Text,
    Other
}

public class UIManager : MonoBehaviour
{
    [SerializeField] private Dictionary<string, Image> inputButtons = new Dictionary<string, Image>();
    [SerializeField] private NecessaryUIs necessaryButtonsList = new NecessaryUIs{uiNames = new Dictionary<string, UIList>(), visibleUINames = new List<NecessaryUI>()};
    
    [SerializeField] private Dictionary<string, TMP_Text> inputTexts = new Dictionary<string, TMP_Text>();
    [SerializeField] private NecessaryUIs necessaryTextsList = new NecessaryUIs{uiNames = new Dictionary<string, UIList>(), visibleUINames = new List<NecessaryUI>()};

    [SerializeField] private Dictionary<string, RectTransform> otherUIs = new Dictionary<string, RectTransform>();
    [SerializeField] private NecessaryUIs necessaryOthersList = new NecessaryUIs{uiNames = new Dictionary<string, UIList>(), visibleUINames = new List<NecessaryUI>()};

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("UIManager OnSceneLoaded "+ GameManager.instance.sceneManager.activeSceneName);
        inputButtons.Clear();
        foreach (var buttonName in necessaryButtonsList[GameManager.instance.sceneManager.activeSceneName].uiNames)
        {
            var go = GameObject.Find(buttonName);
            //Debug.Log(buttonName);
            if (go != null) {
                Debug.Log(buttonName);
                var img = go.GetComponent<Image>();
                if (img != null)
                {
                    inputButtons.Add(buttonName, img);
                }
            }
        }
        inputTexts.Clear();
        foreach (var textName in necessaryTextsList[GameManager.instance.sceneManager.activeSceneName].uiNames)
        {
            var go = GameObject.Find(textName);
            //Debug.Log(textName);
            if (go != null) {
                Debug.Log(textName);
                var txt = go.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    inputTexts.Add(textName, txt);
                }
            }
        }
        otherUIs.Clear();
        foreach (var otherName in necessaryOthersList[GameManager.instance.sceneManager.activeSceneName].uiNames)
        {
            var go = GameObject.Find(otherName);
            //Debug.Log(otherName);
            if (go != null) {
                Debug.Log(otherName);
                var tf = go.GetComponent<RectTransform>();
                if (tf != null)
                {
                    otherUIs.Add(otherName, tf);
                }
            }
        }
    }

    public void SetUIPlace(UIType uiType,string uiName, Vector2 pos)
    {
        switch (uiType)
        {
            case UIType.Button:
                if (inputButtons.ContainsKey(uiName))
                {
                    inputButtons[uiName].rectTransform.anchoredPosition = pos;
                }
                break;
            case UIType.Text:
                if (inputTexts.ContainsKey(uiName))
                {
                    inputTexts[uiName].rectTransform.anchoredPosition = pos;
                }
                break;
            case UIType.Other:
                if (otherUIs.ContainsKey(uiName))
                {
                    otherUIs[uiName].localPosition = pos;
                }
                break;
        }
    }

    public void SetUIActive(UIType uiType, string uiName, bool isActive)
    {
        switch (uiType)
        {
            case UIType.Button:
                if (inputButtons.ContainsKey(uiName))
                {
                    inputButtons[uiName].gameObject.SetActive(isActive);
                }
                break;
            case UIType.Text:
                if (inputTexts.ContainsKey(uiName))
                {
                    inputTexts[uiName].gameObject.SetActive(isActive);
                }
                break;
            case UIType.Other:
                if (otherUIs.ContainsKey(uiName))
                {
                    otherUIs[uiName].gameObject.SetActive(isActive);
                }
                break;
        }
    }

    public void SetUISize(UIType uiType, string uiName, Vector2 size)
    {
        switch (uiType)
        {
            case UIType.Button:
                if (inputButtons.ContainsKey(uiName))
                {
                    inputButtons[uiName].rectTransform.sizeDelta = size;
                }
                break;
            case UIType.Text:
                if (inputTexts.ContainsKey(uiName))
                {
                    inputTexts[uiName].rectTransform.sizeDelta = size;
                }
                break;
            case UIType.Other:
                if (otherUIs.ContainsKey(uiName))
                {
                    otherUIs[uiName].sizeDelta = size;
                }
                break;
        }
    }

    public void SetUIText(string uiName, string text)
    {
        if (inputTexts.ContainsKey(uiName))
        {
            //Debug.Log(uiName+" "+text);
            inputTexts[uiName].SetText(text);
        }
    }

    public void SetUIValueWithDelay(string uiName, BigInteger value)
    {
        StartCoroutine(SetUIValueWithDelayCoroutine(uiName, value));
    }

    private IEnumerator SetUIValueWithDelayCoroutine(string uiName, BigInteger value)
    {
        if (inputTexts.ContainsKey(uiName))
        {
            //Debug.Log("Setting "+uiName+" to "+value);
            int digits = value.ToString().Length-1;
            int sum = value.ToString().ToList().Sum(c => c - '0');
            string currentText = "";
            string allText = value.ToString();
            for (int i = digits; i >= 0; i--)
            {
                decimal c = allText[digits - i] - '0';
                //Debug.Log("Digit "+(digits-i)+": "+c);
                for(int j = 0;j <= c; j++)
                {
                    inputTexts[uiName].SetText(currentText+j.ToString());
                    yield return new WaitForSeconds(0.01f);
                }
                currentText += c.ToString();
            }
        }
        else yield break;
    }

    public void SetImageFillAmount(string uiName, float fillAmount)
    {
        if (inputButtons.ContainsKey(uiName))
        {
            //Debug.Log("Setting fill amount of "+uiName+" to "+fillAmount);
            inputButtons[uiName].fillAmount = fillAmount;
        }
    }
}
