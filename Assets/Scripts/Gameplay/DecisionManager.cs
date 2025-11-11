using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using MyBox;
using Photon.Pun;

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

    public TextButtonInfo(string text, Color color, Action action = null)
    {
        this.text = text;
        this.action = action;
        this.color = color;
    }
}

public class DecisionManager : PhotonCompatible
{
    public static DecisionManager inst;
    [SerializeField] TMP_Text instructionsText;
    [SerializeField] List<Button> textButtons = new();
    HashSet<ButtonSelect> availableUI = new();

    [SerializeField] Button sliderConfirm;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text minimumText;
    [SerializeField] TMP_Text maximumText;
    [SerializeField] TMP_Text currentText;

    protected override void Awake()
    {
        inst = this;
        slider.onValueChanged.AddListener(UpdateText);
        void UpdateText(float value)
        {
            currentText.text = KeywordTooltip.instance.EditText($"{(int)value}");
        }
    }

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

    public void ChooseTextButton(List<TextButtonInfo> possibleChoices, bool autoResolve = true)
    {
        if (possibleChoices.Count == 0 && autoResolve)
        {
        }
        else if (possibleChoices.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => possibleChoices[0].action?.Invoke());
        }
        else
        {
            Log.inst.SetUndoPoint(true);

            for (int i = 0; i<textButtons.Count; i++)
            {
                Button nextButton = textButtons[i];
                if (i < possibleChoices.Count )
                {
                    nextButton.gameObject.SetActive(true);
                    nextButton.name = possibleChoices[i].text;

                    string translatedText = Translator.inst.SplitAndTranslate(-1, possibleChoices[i].text);
                    nextButton.transform.GetChild(0).GetComponent<TMP_Text>().text = translatedText;
                    nextButton.image.color = possibleChoices[i].color;

                    int number = i;
                    nextButton.onClick.AddListener(Resolve);

                    void Resolve()
                    {
                        if (possibleChoices[i].action != null) Log.inst.inReaction.Add(possibleChoices[i].action);
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

    public void ChooseCardOnScreen(List<Card> listOfCards, Action<Card> action = null, bool autoResolve = true)
    {
        if (listOfCards.Count == 0 && autoResolve)
        {
        }
        else if (listOfCards.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfCards[0]));
        }
        else
        {
            Log.inst.SetUndoPoint(true);

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

    public void ChooseDisplayOnScreen(List<MiniCardDisplay> listOfDisplays, Action<Card> action = null, bool autoResolve = true)
    {
        if (listOfDisplays.Count == 0 && autoResolve)
        {
        }
        else if (listOfDisplays.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfDisplays[0].card));
        }
        else
        {
            Log.inst.SetUndoPoint(true);

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

    public void ChooseFromSlider(int min, int max, Action<int> action, bool autoResolve = true)
    {
        if (min == max && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(min));
        }
        else
        {
            Log.inst.SetUndoPoint(true);
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

    [PunRPC]
    public string Instructions(string text)
    {
        string answer = Translator.inst.SplitAndTranslate(-1, text, 0);
        instructionsText.text = answer;
        return answer;
    }
}
