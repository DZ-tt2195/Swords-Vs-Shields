using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class MiniCardDisplay : MonoBehaviour, IPointerClickHandler
{
    public Button button { get; private set; }
    public Card card { get; private set; }
    [SerializeField] Image image;
    [SerializeField] TMP_Text description;
    [SerializeField] Image drawX;
    public Image border;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            CarryVariables.inst.RightClickDisplay(card.thisCard.dataFile, 1, 0);
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    private void FixedUpdate()
    {
        try { this.border.SetAlpha(PlayerCreator.inst.opacity); } catch { }
    }

    public void NewCard(Card card)
    {
        this.card = card;
        image.sprite = card.thisCard.dataFile.sprite;

        string text = "";
        description.transform.parent.gameObject.SetActive(!text.Equals(""));
        description.text = KeywordTooltip.instance.EditText(text);
    }
}
