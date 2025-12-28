using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using MyBox;

[Serializable]
public class KeywordHover
{
    public string original;
    [ReadOnly] public string translated;
    [ReadOnly] public string description;
    public Color color = Color.white;
}

public class KeywordTooltip : MonoBehaviour
{
    public static KeywordTooltip instance;
    float XCap;
    float Ydisplace;

    [SerializeField] List<KeywordHover> linkedKeywords = new();
    [SerializeField] List<KeywordHover> spriteKeywords = new();
    [SerializeField] TMP_Text tooltipText;
    Dictionary<string, CardLayout> listOfCardRC = new();


    private void Awake()
    {
        instance = this;
        XCap = tooltipText.rectTransform.sizeDelta.x / 2f;
        Ydisplace = tooltipText.rectTransform.sizeDelta.y * 1.25f;
    }

    public void SwitchLanguage()
    {
        foreach (KeywordHover hover in linkedKeywords)
        {
            hover.translated = AutoTranslate.DoEnum((ToTranslate)Enum.Parse(typeof(ToTranslate), hover.original));
            hover.description = Translator.inst.Translate($"{hover.original}_Text");
        }
        foreach (KeywordHover hover in spriteKeywords)
        {
            hover.translated = AutoTranslate.DoEnum((ToTranslate)Enum.Parse(typeof(ToTranslate), hover.original));
            hover.description = Translator.inst.Translate($"{hover.original}_Text");
        }

        foreach (KeywordHover hover in linkedKeywords)
            hover.description = EditText(hover.description);
        foreach (KeywordHover hover in spriteKeywords)
            hover.description = EditText(hover.description);
    }

    public string EditText(string text)
    {
        if (text.Length == 0)
            return "";

        string answer = Regex.Replace(text, "(?<=[a-z])(?=[A-Z])", " ");
        answer = Regex.Replace(answer, @",(\s*(\n|$))", "$1");
        answer = Regex.Replace(answer, @"-(\s*(\n|$))", "$1");
        answer = answer.Replace("-u003e", "->");

        answer = text;
        foreach (KeywordHover link in linkedKeywords)
        {
            string toReplace = link.translated;
            answer = answer.Replace(toReplace, $"<link=\"{link.translated}\"><u>" +
                $"<color=#{ColorUtility.ToHtmlStringRGB(link.color)}>{link.translated}<color=#FFFFFF></u></link>");
        }
        foreach (KeywordHover link in spriteKeywords)
        {
            string toReplace = link.translated;
            answer = answer.Replace(toReplace, $"<link=\"{link.original}\"><sprite=\"{link.original}\" name=\"{link.original}\"></link>");
        }
        return answer;
    }

    public KeywordHover SearchForKeyword(string target)
    {
        foreach (KeywordHover link in linkedKeywords)
        {
            if (link.translated.Equals(target))
                return link;
        }
        foreach (KeywordHover link in spriteKeywords)
        {
            if (link.translated.Equals(target))
                return link;
        }
        Debug.LogError($"{target} couldn't be found");
        return null;
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

    public void ActivateTextBox(string target, Vector3 mousePosition)
    {
        this.transform.SetAsLastSibling();

        foreach (KeywordHover entry in linkedKeywords)
        {
            if (entry.original.Equals(target))
            {
                tooltipText.text = entry.description;
                tooltipText.transform.parent.position = CalculatePosition(mousePosition);
                tooltipText.transform.parent.gameObject.SetActive(true);
                return;
            }
        }
        foreach (KeywordHover entry in spriteKeywords)
        {
            if (entry.original.Equals(target))
            {
                tooltipText.text = entry.description;
                tooltipText.transform.parent.position = CalculatePosition(mousePosition);
                tooltipText.transform.parent.gameObject.SetActive(true);
                return;
            }
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