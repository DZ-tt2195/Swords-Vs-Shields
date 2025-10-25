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
        CarryVariables.inst.RightClickDisplay(storedData, alpha, rotation);
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

        background.color = dataFile.startingArea switch
        {
            CardAreas.Coast => Color.blue,
            CardAreas.City => Color.gray,
            CardAreas.Woods => Color.forestGreen,
            CardAreas.Village => Color.red,
            _ => Color.black
        };

        cardName.text = $"{Translator.inst.Translate(dataFile.cardName)} - {dataFile.startingBox} Box";
        cardArt.sprite = dataFile.sprite;
        string textOne = Translator.inst.Translate($"{dataFile.cardName} TextOne");
        string textTwo = Translator.inst.Translate($"{dataFile.cardName} TextTwo");

        if (textTwo.Equals(""))
        {
            UseBigBox(true);
            bigText.textBox.text = KeywordTooltip.instance.EditText(textOne);
        }
        else
        {
            UseBigBox(false);
            smallTextOne.textBox.text = KeywordTooltip.instance.EditText(textOne);
            smallTextTwo.textBox.text = KeywordTooltip.instance.EditText(textTwo);
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
