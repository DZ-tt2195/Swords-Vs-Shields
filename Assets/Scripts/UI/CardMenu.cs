using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using System.Text.RegularExpressions;

public class CardMenu : MonoBehaviour
{
    public static CardMenu instance;
    int step = 0;

    [SerializeField] Button confirmButton;
    [SerializeField] GridLayoutGroup storeButtons;
    CardSelect mostRecentClick;
    List<Button> blankButtons = new();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        Advance();
        confirmButton.onClick.AddListener(Advance);
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom.MaxPlayers >= 2)
            this.gameObject.SetActive(false);
    }

    public void ChooseFromList(CardSelect clicked, List<string> cardNames, bool vertical)
    {
        mostRecentClick = clicked;
        if (vertical)
        {
            storeButtons.constraintCount = 4;
            storeButtons.spacing = new(30, 30);
        }
        else
        {
            storeButtons.constraintCount = 2;
            storeButtons.spacing = new(200, -80);
        }

        for (int i = 0; i < blankButtons.Count; i++)
        {
            Button button = blankButtons[i];
            try
            {
                string answer = Regex.Replace(cardNames[i], "(?<=[a-z])(?=[A-Z])", " ");
                Sprite data = Resources.Load<Sprite>($"Card Art/{answer}");

                //button.GetComponent<CardLayout>().FillInCards(data, 1, vertical ? 0 : -90);
                button.gameObject.SetActive(true);
            }
            catch
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    void SendName(int number)
    {
        mostRecentClick.SetCardImage(number);
        mostRecentClick = null;
        foreach (Button button in blankButtons)
            button.gameObject.SetActive(false);
    }

    void Advance()
    {
        if (step == 0)
        {
            for (int i = 0; i < storeButtons.transform.childCount; i++)
            {
                Button nextButton = storeButtons.transform.GetChild(i).gameObject.GetComponent<Button>();
                blankButtons.Add(nextButton);
                nextButton.interactable = true;
                nextButton.onClick.RemoveAllListeners();
                int number = i;
                nextButton.onClick.AddListener(() => SendName(number));
                nextButton.gameObject.SetActive(false);
            }
        }
        else
        {
            PlayerPrefs.Save();
            this.gameObject.SetActive(false);
        }
        step++;
    }
}
