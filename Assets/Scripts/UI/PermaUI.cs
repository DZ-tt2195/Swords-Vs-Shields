using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;
using TMPro;

public class PermaUI : MonoBehaviour
{

#region Setup

    public static PermaUI inst;

    [SerializeField] Transform rightClickBackground;
    [SerializeField] CardLayout rightClickCard;
    [SerializeField] TMP_Text artistCredit;
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

    public void RightClickDisplay(CardData dataFile, bool visible, float rotation)
    {
        rightClickBackground.gameObject.SetActive(true);
        rightClickCard.gameObject.SetActive(true);
        rightClickCard.FillInCards(dataFile, visible ? 1f : 0f, rotation);
        artistCredit.text = dataFile.artCredit;
    }

    #endregion

}
