using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using TMPro;

public class CarryVariables : MonoBehaviour
{

#region Setup

    public static CarryVariables inst;

    [Foldout("Prefabs", true)]
    public Player playerPrefab;
    public CardLayout cardPrefab;
    public CardPopup cardPopup;
    public Button cardButton;
    public Sprite faceDown;

    [Foldout("Right click", true)]
    [SerializeField] Transform rightClickBackground;
    [SerializeField] CardLayout rightClickCard;
    [SerializeField] TMP_Text artistCredit;

    [Foldout("Misc", true)]
    [SerializeField] Transform permanentCanvas;

    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(this.gameObject);
            rightClickBackground.transform.gameObject.SetActive(false);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public string PrintIntList(List<int> listOfInts)
    {
        string answer = "";
        foreach (int next in listOfInts)
            answer += $"{next}, ";
        return answer;
    }

    #endregion

#region Right click

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            rightClickBackground.gameObject.SetActive(false);
    }

    public void RightClickDisplay(CardData dataFile, float alpha, float rotation)
    {
        rightClickBackground.gameObject.SetActive(true);
        rightClickCard.gameObject.SetActive(true);
        rightClickCard.FillInCards(dataFile, alpha, rotation);
        artistCredit.text = dataFile.artCredit;
    }

    #endregion

}
