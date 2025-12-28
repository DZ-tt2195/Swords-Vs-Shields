using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using MyBox;
using Photon.Pun;

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

public class TextButtonInfo
{
    public string toFind;
    public string playerName;
    public string cardName;
    public string number;
    public Action action;
    public Color color;

    public TextButtonInfo(string toFind, string playerName, string cardName, string number, Action action = null)
    {
        this.toFind = toFind;
        this.playerName = playerName;
        this.cardName = cardName;
        this.number = number;
        this.action = action;
        this.color = Color.white;
    }

    public TextButtonInfo(string toFind, string playerName, string cardName, string number, Color color, Action action = null)
    {
        this.toFind = toFind;
        this.playerName = playerName;
        this.cardName = cardName;
        this.number = number;
        this.action = action;
        this.color = color;
    }
}

public class MakeDecision : PhotonCompatible
{

#region Setup

    public static MakeDecision inst;
    [SerializeField] TMP_Text instructionsText;
    [SerializeField] Transform findTextButtons;
    List<Button> textButtons = new();
    HashSet<ButtonSelect> availableUI = new();

    [SerializeField] Button sliderConfirm;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text minimumText;
    [SerializeField] TMP_Text maximumText;
    [SerializeField] TMP_Text currentText;

    protected override void Awake()
    {
        base.Awake();
        inst = this;
        this.bottomType = this.GetType();
        slider.onValueChanged.AddListener(UpdateText);

        foreach (Transform child in findTextButtons)
        {
            Button button = child.GetComponent<Button>();
            textButtons.Add(button);
            button.gameObject.SetActive(false);
        }

        void UpdateText(float value)
        {
            currentText.text = KeywordTooltip.instance.EditText($"{(int)value}");
        }
    }

    #endregion

#region Decisions

    public void ChooseTextButton(List<TextButtonInfo> possibleChoices, string toFind, string playerName, string cardName, string number, bool autoResolve = true)
    {
        if (possibleChoices.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => possibleChoices[0].action?.Invoke());
        }
        else if (possibleChoices.Count >= 1 || !autoResolve)
        {
            Log.inst.SetUndoPoint(true);
            Instructions(toFind, playerName, cardName, number);

            for (int i = 0; i<textButtons.Count; i++)
            {
                Button nextButton = textButtons[i];
                if (i < possibleChoices.Count)
                {
                    TextButtonInfo info = possibleChoices[i];
                    nextButton.gameObject.SetActive(true);
                    nextButton.name = info.toFind;

                    string translatedText = Translator.inst.Packaging(info.toFind, info.playerName, info.cardName, info.number);
                    nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = translatedText;
                    nextButton.image.color = info.color;
                    nextButton.onClick.AddListener(Resolve);

                    void Resolve()
                    {
                        Log.inst.inReaction.Add(() => info.action?.Invoke());
                        Log.inst.PopStack();
                    }
                }
                else
                {
                    nextButton.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ChooseCardOnScreen(List<Card> listOfCards, string toFind, string playerName, string cardName, string number, Action<Card> action = null, bool autoResolve = true)
    {
        if (listOfCards.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfCards[0]));
        }
        else if (listOfCards.Count >= 1 || !autoResolve)
        {
            Log.inst.SetUndoPoint(true);
            Instructions(toFind, playerName, cardName, number);

            for (int j = 0; j < listOfCards.Count; j++)
            {
                Card nextCard = listOfCards[j];
                availableUI.Add(nextCard.selectMe);
                Button cardButton = nextCard.selectMe.button;

                cardButton.interactable = true;
                nextCard.selectMe.border.gameObject.SetActive(true);
                cardButton.onClick.AddListener(ClickedThis);

                void ClickedThis()
                {
                    Log.inst.inReaction.Add(() => action?.Invoke(nextCard));
                    Log.inst.PopStack();
                }
            }
        }
    }

    public void ChooseDisplayOnScreen(List<MiniCardDisplay> listOfDisplays, string toFind, string playerName, string cardName, string number, Action<Card> action = null, bool autoResolve = true)
    {
        if (listOfDisplays.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfDisplays[0].card));
        }
        else if (listOfDisplays.Count >= 1 || !autoResolve)
        {
            Log.inst.SetUndoPoint(true);
            Instructions(toFind, playerName, cardName, number);

            for (int j = 0; j < listOfDisplays.Count; j++)
            {
                MiniCardDisplay nextDisplay = listOfDisplays[j];
                availableUI.Add(nextDisplay.selectMe);
                Button cardButton = nextDisplay.selectMe.button;

                cardButton.interactable = true;
                nextDisplay.selectMe.border.gameObject.SetActive(true);
                cardButton.onClick.AddListener(ClickedThis);

                void ClickedThis()
                {
                    Log.inst.inReaction.Add(() => action?.Invoke(nextDisplay.card));
                    Log.inst.PopStack();
                }
            }
        }

    }

    public void ChooseFromSlider(int min, int max, string toFind, string playerName, string cardName, string number, Action<int> action = null, bool autoResolve = true)
    {
        if (min == max && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(min));
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            Instructions(toFind, playerName, cardName, number);

            slider.gameObject.SetActive(true);
            sliderConfirm.onClick.AddListener(DecisionMade);

            minimumText.text = min.ToString();
            slider.minValue = min;
            maximumText.text = max.ToString();
            slider.maxValue = max;

            slider.value = min;
            void DecisionMade()
            {
                Log.inst.inReaction.Add(() => action?.Invoke((int)slider.value));
                Log.inst.PopStack();
            }
        }
    }

    /*
    public void ChooseCardInPopup(List<CardButtonInfo> possibleCards, string instructions, bool autoResolve = true)
    {
        if (possibleCards.Count == 1 && autoResolve)
        {
            CardButtonInfo onlyOne = possibleCards[0];
            Log.inst.inReaction.Add(() => onlyOne.action?.Invoke(onlyOne.card));
        }
        else if (possibleCards.Count >= 1 || !autoResolve)
        {
            Log.inst.SetUndoPoint(true);
                      Instructions(instructions);
  for (int i = 0; i < textButtons.Count; i++)
            {
                Button nextButton = textButtons[i];
                if (i < possibleCards.Count)
                {
                    CardButtonInfo info = possibleCards[i];
                    nextButton.gameObject.SetActive(true);
                    nextButton.name = possibleCards[i].card.name;

                    nextButton.onClick.AddListener(Resolve);

                    void Resolve()
                    {
                        Log.inst.inReaction.Add(() => info.action?.Invoke(info.card));
                        Log.inst.PopStack();
                    }
                }
                else
                {
                    nextButton.gameObject.SetActive(false);
                }
            }
        }
    }
    */
    #endregion

#region Misc

    public void ClearDecisions()
    {
        foreach (Button button in textButtons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }
        foreach (ButtonSelect select in availableUI)
        {
            select.button.onClick.RemoveAllListeners();
            select.button.interactable = false;
            select.border.gameObject.SetActive(false);
        }
        availableUI.Clear();
        slider.gameObject.SetActive(false);

        instructionsText.text = "";
    }

    [PunRPC]
    public string Instructions(string toFind, string playerName, string cardName, string number)
    {
        string answer = Translator.inst.Packaging(toFind, playerName, cardName, number);
        instructionsText.text = answer;
        return answer;
    }

#endregion

}
