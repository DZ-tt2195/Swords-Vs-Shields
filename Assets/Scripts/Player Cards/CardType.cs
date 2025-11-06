using System.Text.RegularExpressions;
using UnityEngine;
using System;

public enum AbilityType { None, Play, Defend, Attack }
public class CardType
{
    public CardData dataFile { get; private set; }

    public CardType(CardData dataFile)
    {
        this.dataFile = dataFile;
    }

    public virtual AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        return dataFile.typeOne;
    }

    public virtual void DoAbilityOne(Player player, Card thisCard, int logged)
    {

    }

    public virtual AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        return dataFile.typeTwo;
    }

    public virtual void DoAbilityTwo(Player player, Card thisCard, int logged)
    {

    }
}
