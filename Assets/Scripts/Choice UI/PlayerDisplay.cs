using UnityEngine;
using TMPro;

public class PlayerDisplay : MonoBehaviour
{
    public ButtonSelect selectMe { get; private set; }
    public Player player { get; private set; }
    [SerializeField] TMP_Text heartText;
    [SerializeField] TMP_Text descriptionText;

    public void AssignInfo(Player player, int heart, string desc)
    {
        this.player = player;
        heartText.text = $"{heart}";
        descriptionText.text = desc;
    }
}
