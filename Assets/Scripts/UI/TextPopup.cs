using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TextButtonInfo
{
    public string text;
    public Action action;
    public Color color;

    public TextButtonInfo(string text, Action action = null)
    {
        this.text = text;
        this.action = action;
        this.color = Color.white;
    }

    public TextButtonInfo (string text, Color color, Action action = null)
    {
        this.text = text;
        this.action = action;
        this.color = color;
    }
}

public class TextPopup : Popup
{
    public override void StatsSetup(bool destroyMe, string header, Vector2 position)
    {
        base.StatsSetup(destroyMe, header, position);

        if (header == "")
        {
            textbox.gameObject.SetActive(false);
            imageWidth.sizeDelta = new Vector2(imageWidth.sizeDelta.x, imageWidth.sizeDelta.y / 2);
        }
        else
        {
            this.name = header;
            this.textbox.text = KeywordTooltip.instance.EditText(header);
        }
    }

    internal int AddTextButton(TextButtonInfo info)
    {
        Button nextButton = Instantiate(CarryVariables.inst.textButton, this.storeThings);
        nextButton.name = info.text;
        nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = KeywordTooltip.instance.EditText(info.text);
        nextButton.image.color = info.color;

        nextButton.interactable = true;
        int number = buttonsInCollector.Count;
        nextButton.onClick.AddListener(Resolve);
        buttonsInCollector.Add(nextButton);

        Resize();
        return number;

        void Resolve()
        {
            if (info.action != null)
            {
                Log.inst.inReaction.Add(info.action);
                //Debug.Log($"add in {info.action}");
            }
            Log.inst.PopStack();
        }
    }
}
