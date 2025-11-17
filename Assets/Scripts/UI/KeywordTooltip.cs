using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;
using MyBox;

[Serializable]
public class KeywordHover
{
    public string englishString;
    [ReadOnly] public string translatedString;
    [ReadOnly] public string description;
    public Color color = Color.white;
}

public class KeywordTooltip : MonoBehaviour
{
    public static KeywordTooltip instance;
    float XCap;
    float Ydisplace;
    [SerializeField] TMP_Text tooltipText;

    [SerializeField] List<KeywordHover> linkedKeywords = new();
    [SerializeField] List<KeywordHover> spriteKeywords = new();
    Dictionary<string, CardLayout> listOfCardRC = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            XCap = tooltipText.rectTransform.sizeDelta.x / 2f;
            Ydisplace = tooltipText.rectTransform.sizeDelta.y * 1.25f;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SwitchLanguage()
    {
        foreach (KeywordHover hover in linkedKeywords)
        {
            hover.translatedString = Translator.inst.Translate(hover.englishString);
            if (Translator.inst.TranslationExists($"{hover.englishString} Text"))
                hover.description = Translator.inst.Translate($"{hover.englishString} Text");
        }
        foreach (KeywordHover hover in spriteKeywords)
        {
            hover.translatedString = Translator.inst.Translate(hover.englishString);
            if (Translator.inst.TranslationExists($"{hover.englishString} Text"))
            hover.description = Translator.inst.Translate($"{hover.englishString} Text");
        }

        foreach (KeywordHover hover in linkedKeywords)
            hover.description = EditText(hover.description);
        foreach (KeywordHover hover in spriteKeywords)
            hover.description = EditText(hover.description);
    }

    public string EditText(string text)
    {
        if (text.Equals(""))
            return "";

        string answer = Regex.Replace(text, "(?<=[a-z])(?=[A-Z])", " ");
        answer = Regex.Replace(answer, @",(\s*(\n|$))", "$1");
        answer = Regex.Replace(answer, @"-(\s*(\n|$))", "$1");
        answer = answer.Replace("-u003e", "->");

        foreach (KeywordHover link in linkedKeywords)
        {
            string toReplace = link.translatedString;
            answer = answer.Replace(toReplace, $"<link=\"{toReplace}\"><u>" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(link.color)}>{toReplace}<color=#FFFFFF></u></link>");
        }
        foreach (KeywordHover link in spriteKeywords)
        {
            string toReplace = link.translatedString;
            answer = answer.Replace(toReplace, $"<link=\"{link.englishString}\"><sprite=\"{link.englishString}\" name=\"{link.englishString}\"></link>");
        }
        foreach (var next in listOfCardRC)
        {
            string toReplace = next.Key;
            answer = answer.Replace(toReplace, $"<link=\"{toReplace}\"><i>{toReplace}</i></link>");
        }
        return answer;
    }

    private void Update()
    {
        tooltipText.transform.parent.gameObject.SetActive(false);
    }

    Vector3 CalculatePosition(Vector3 mousePosition)
    {
        return new Vector3
            (Mathf.Clamp(mousePosition.x, XCap, Screen.width - XCap),
            mousePosition.y + (mousePosition.y > Ydisplace ? -0.5f : 0.5f) * Ydisplace,
            0);
    }

    public KeywordHover SearchForKeyword(string target)
    {
        foreach (KeywordHover link in linkedKeywords)
        {
            if (link.translatedString.Equals(target))
                return link;
        }
        foreach (KeywordHover link in spriteKeywords)
        {
            if (link.translatedString.Equals(target))
                return link;
        }
        Debug.LogError($"{target} couldn't be found");
        return null;
    }

    public void ActivateTextBox(string target, Vector3 mousePosition)
    {
        this.transform.SetAsLastSibling();

        foreach (KeywordHover entry in linkedKeywords)
        {
            if (Display(entry))
                return;
        }
        foreach (KeywordHover entry in spriteKeywords)
        {
            if (Display(entry))
                return;
        }

        bool Display(KeywordHover keyword)
        {
            if (keyword.englishString.Equals(target) && !keyword.description.Equals(""))
            {
                tooltipText.text = keyword.description;
                tooltipText.transform.parent.position = CalculatePosition(mousePosition);
                tooltipText.transform.parent.gameObject.SetActive(true);
                return true;
            }
            return false;
        }
    }

    public void NewCardRC(string cardName, CardLayout layout)
    {
        listOfCardRC[cardName] = layout;
    }

    public CardLayout FindCardRC(string cardName)
    {
        return listOfCardRC[cardName];
    }
}
