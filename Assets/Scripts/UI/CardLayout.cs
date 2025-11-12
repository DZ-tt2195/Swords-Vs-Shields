using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TextPairing
{
    public Image image;
    public TMP_Text textBox;
}

public class CardLayout : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] Image cardArt;
    [SerializeField] Image background;
    [SerializeField] TMP_Text cardName;

    [SerializeField] TextPairing smallTextOne;
    [SerializeField] TextPairing smallTextTwo;
    [SerializeField] TextPairing bigText;

    CardData storedData;
    float rotation;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            RightClickedMe(cg.alpha);
    }

    public void RightClickedMe(float alpha)
    {
        //Debug.Log(artBox.sprite.name);
        PermaUI.inst.RightClickDisplay(storedData, alpha, rotation);
    }

    public float GetAlpha()
    {
        return cg.alpha;
    }

    public void FillInCards(CardData dataFile, float alpha, float rotation)
    {
        storedData = dataFile;
        cg.alpha = alpha;
        this.transform.localEulerAngles = new(0, 0, rotation);
        this.rotation = rotation;

        cardName.text = KeywordTooltip.instance.EditText($"{Translator.inst.Translate(dataFile.cardName)} {dataFile.startingHealth} Health");
        cardArt.sprite = dataFile.sprite;
        string textOne = Translator.inst.Translate($"{dataFile.cardName} TextOne");
        string textTwo = Translator.inst.Translate($"{dataFile.cardName} TextTwo");

        if (dataFile.typeTwo == AbilityType.None)
        {
            UseBigBox(true);
            ApplyText(bigText, textOne, dataFile.typeOne);

        }
        else
        {
            UseBigBox(false);
            ApplyText(smallTextOne, textOne, dataFile.typeOne);
            ApplyText(smallTextTwo, textTwo, dataFile.typeTwo);
        }
    }

    void ApplyText(TextPairing pairing, string text, AbilityType type)
    {
        pairing.textBox.text = KeywordTooltip.instance.EditText(text);
        switch (type)
        {
            case AbilityType.Defend:
                pairing.image.color = new Color(0, 0.66f, 0); //green
                break;
            case AbilityType.Attack:
                pairing.image.color = new Color(1, 0.33f, 0.33f); //light red
                break;
            case AbilityType.Play:
                pairing.image.color = new Color(0.66f, 0.66f, 0.66f); //gray
                break;
        }
    }

    void UseBigBox(bool yes)
    {
        bigText.image.gameObject.SetActive(yes);
        bigText.textBox.gameObject.SetActive(yes);
        smallTextOne.image.gameObject.SetActive(!yes);
        smallTextOne.textBox.gameObject.SetActive(!yes);
        smallTextTwo.image.gameObject.SetActive(!yes);
        smallTextTwo.textBox.gameObject.SetActive(!yes);
    }
}
