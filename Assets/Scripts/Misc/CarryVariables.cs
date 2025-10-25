using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.IO;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine.Networking;

public class CarryVariables : MonoBehaviour
{

#region Setup

    public static CarryVariables inst;

    [Foldout("Prefabs", true)]
    public Player playerPrefab;
    public CardLayout cardPrefab;
    public TextPopup textPopup;
    public CardPopup cardPopup;
    public Button textButton;
    public Button cardButton;
    public SliderChoice sliderPopup;
    public Sprite faceDown;

    [Foldout("Right click", true)]
    [SerializeField] Transform rightClickBackground;
    [SerializeField] CardLayout rightClickCard;

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

            Test(2, 6, 1);

            void Test(int numPlayers, int numRounds, int toRemove)
            {
                List<int> list = new();
                for (int i = 1; i <= numPlayers; i++)
                    list.Add(i);

                int index = list.IndexOf(toRemove);
                list.RemoveAt(index);

                for (int i = 1; i <= numRounds; i++)
                {
                    int firstAnswer = (index - 1 + i) % list.Count;
                    int secondAnswer = (index - i);

                    while (secondAnswer < 0)
                    {
                        secondAnswer += (list.Count);
                    }
                    Debug.Log($"Round {i}: {list[firstAnswer]}, {list[secondAnswer]}");
                }
            }
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
    }

    #endregion

}
