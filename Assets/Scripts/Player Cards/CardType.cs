using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;
using System;

public enum AbilityType { None, Play, Defend, Attack }
public class CardType
{
    public CardData dataFile { get; private set; }

    public CardType(CardData dataFile)
    {
        this.dataFile = dataFile;
    }

    protected virtual AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        return dataFile.typeOne;
    }

    protected virtual void DoAbilityOne(Player player, Card thisCard, int logged)
    {

    }

    protected virtual AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        return dataFile.typeTwo;
    }

    protected virtual void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
    }

    public bool HasType(AbilityType toFind, Player player, Card thisCard, int logged)
    {
        if (CanUseAbiltyOne(player, thisCard) == toFind)
        {
            if (logged >= 0)
                Log.inst.NewDecisionContainer(() => DoAbilityOne(player, thisCard, logged), logged);
            return true;
        }
        if (CanUseAbiltyTwo(player, thisCard) == toFind)
        {
            if (logged >= 0)
                Log.inst.NewDecisionContainer(() => DoAbilityTwo(player, thisCard, logged), logged);
            return true;
        }
        return false;
    }
}
