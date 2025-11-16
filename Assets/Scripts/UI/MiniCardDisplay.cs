using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class MiniCardDisplay : MonoBehaviour, IPointerClickHandler
{
    public Card card { get; private set; }
    public ButtonSelect selectMe { get; private set; }
    [SerializeField] Image image;
    [SerializeField] TMP_Text description;
    [SerializeField] Image drawX;

    private void Awake()
    {
        selectMe = GetComponent<ButtonSelect>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            PermaUI.inst.RightClickDisplay(card.thisCard.dataFile, true, 0);
    }

    public void NewCard(Card card)
    {
        this.card = card;
        image.sprite = card.thisCard.dataFile.sprite;

        int currentHealth = card.GetHealth();
        drawX.gameObject.SetActive(currentHealth <= 0);

        string text = $"{currentHealth} Health";
        if (!card.CanUseAbility())
            text += $" {Translator.inst.Translate("Stunned")}";
        if (!card.CanTakeDamage())
            text += $" {Translator.inst.Translate("Protected")}";

        description.text = KeywordTooltip.instance.EditText(text);
    }
}
