using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class CardButtonInfo
{
    public Card card;
    public Action<Card> action;
    public float alpha;
    public bool clickable;

    public CardButtonInfo(Card card, Action<Card> action = null, float alpha = 1f, bool clickable = true)
    {
        this.card = card;
        this.action = action;
        this.alpha = alpha;
        this.clickable = clickable;
    }

    public CardData GetFile()
    {
        return this.card.thisCard.dataFile;
    }
}

public class CardPopup : Popup
{
    public override void StatsSetup(bool destroyMe, string header, Vector2 position)
    {
        base.StatsSetup(destroyMe, header, position);

        if (header == "")
        {
            textbox.gameObject.SetActive(false);
            imageWidth.sizeDelta = new Vector2(imageWidth.sizeDelta.x, imageWidth.sizeDelta.y * 0.75f);
            this.storeThings.transform.localPosition = new(0, 100);
        }
        else
        {
            this.name = header;
            this.textbox.text = header;
        }
    }

    internal int AddCardButton(CardButtonInfo info)
    {
        Button nextButton = Instantiate(CarryVariables.inst.cardButton, this.storeThings);
        nextButton.name = info.card.name;
        CardLayout layout = nextButton.GetComponent<CardLayout>();
        layout.FillInCards(info.GetFile(), info.alpha, 0);

        nextButton.interactable = info.clickable;
        int number = buttonsInCollector.Count;
        nextButton.onClick.AddListener(Resolve);
        buttonsInCollector.Add(nextButton);

        Resize();
        return number;

        void Resolve()
        {
            if (info.action != null)
            {
                Log.inst.inReaction.Add(() => info.action(info.card));
                //Debug.Log($"add in {info.action}");
            }
            Log.inst.PopStack();
        }
    }
}
