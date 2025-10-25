using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CardSelect : MonoBehaviour
{
    RectTransform rectTrans;
    CardLayout layout;
    Button randomButton;
    Button chooseButton;

    List<string> cardNames;
    bool vertical;

    private void Awake()
    {
        rectTrans = GetComponent<RectTransform>();
        layout = GetComponent<CardLayout>();

        randomButton = transform.Find("Random").GetComponent<Button>();
        randomButton.onClick.AddListener(() => SetCardImage(-1));
        chooseButton = transform.Find("Choose").GetComponent<Button>();

        //cardNames = CarryVariables.inst.cardNames;
        vertical = true;
        chooseButton.onClick.AddListener(() => CardMenu.instance.ChooseFromList(this, cardNames, vertical));
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(this.name) && PlayerPrefs.GetInt(this.name) >= 0)
        {
            SetCardImage(PlayerPrefs.GetInt(this.name));
        }
        else
        {
            SetCardImage(-1);
        }
    }

    public void SetCardImage(int number)
    {
        if (number < 0)
        {
            PlayerPrefs.SetInt(this.name, -1);
            layout.FillInCards(null, 0, vertical ? 0 : -90);
        }
        else
        {
            PlayerPrefs.SetInt(this.name, number);
            string answer = Regex.Replace(cardNames[number], "(?<=[a-z])(?=[A-Z])", " ");

            Sprite sprite = Resources.Load<Sprite>($"Card Art/{answer}");
            //layout.FillInCards(sprite, 1, vertical ? 0 : -90);
        }
        PlayerPrefs.Save();
    }
}
