using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoinRoomButton : MonoBehaviour
{
    public Button button { get; private set; }
    public TMP_Text thisName { get; private set; }
    public TMP_Text playerCount { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();
        thisName = transform.Find("Name").GetComponent<TMP_Text>();
        playerCount = transform.Find("Player Count").GetComponent<TMP_Text>();
    }

    public void ClearInfo()
    {
        button.onClick.RemoveAllListeners();
        thisName.text = "";
        playerCount.text = "";
        this.transform.SetParent(null);
    }
}
