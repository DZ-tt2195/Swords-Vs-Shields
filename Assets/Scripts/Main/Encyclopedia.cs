using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using MyBox;
using Photon.Pun;

public class Encyclopedia : MonoBehaviour
{
    public static Encyclopedia inst;
    [SerializeField] Card cardPrefab;
    [SerializeField] RectTransform groupUI;
    [SerializeField] EditDropdown typeOneDropdown;
    [SerializeField] EditDropdown typeTwoDropdown;
    List<Card> allCards = new();

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        typeOneDropdown.dropdown.onValueChanged.AddListener(NewSort);
        typeTwoDropdown.dropdown.onValueChanged.AddListener(NewSort);

        for (int i = 0; i < Translator.inst.playerCardFiles.Count; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                GameObject nextCard = Instantiate(cardPrefab.gameObject);
                Card cardPV = nextCard.GetComponent<Card>();
                cardPV.AssignCard(Translator.inst.playerCardFiles[i], 1f);
                allCards.Add(cardPV);
            }
        }
        NewSort(0);
    }

    void NewSort(int n)
    {
        string dropdownOne = typeOneDropdown.GetOriginal();
        string dropdownTwo = typeTwoDropdown.GetOriginal();

        foreach (Card card in allCards)
        {
            bool include = true;

            if (!Matches(dropdownOne))
                include = false;
            else if (!Matches(dropdownTwo))
                include = false;

            bool Matches(string text)
            {
                return text switch
                {
                    "Play" => card.thisCard.dataFile.typeOne == AbilityType.Play || card.thisCard.dataFile.typeTwo == AbilityType.Play,
                    "Defend" => card.thisCard.dataFile.typeOne == AbilityType.Defend || card.thisCard.dataFile.typeTwo == AbilityType.Defend,
                    "Attack" => card.thisCard.dataFile.typeOne == AbilityType.Attack || card.thisCard.dataFile.typeTwo == AbilityType.Attack,
                    _ => true  // no filter
                };
            }

            card.transform.SetParent(include ? groupUI : null);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(groupUI);
    }
}
