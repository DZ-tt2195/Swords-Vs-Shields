using System.Text.RegularExpressions;
using UnityEngine;
using System;

public enum AbilityType { AffectYou, AffectOther, Misc, None }
public class CardType
{
    public CardData dataFile { get; private set; }

    public CardType(CardData dataFile)
    {
        this.dataFile = dataFile;
    }

    protected void MakeDecision(Card card, Action action, int logged)
    {
        Log.inst.NewDecisionContainer(card, () => card.MakeDecision(action), logged);
    }

    public virtual AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        return AbilityType.None;
    }

    public virtual void DoAbilityOne(Player player, Card thisCard, int logged)
    {

    }

    public virtual AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        return AbilityType.None;
    }

    public virtual void DoAbilityTwo(Player player, Card thisCard, int logged)
    {

    }
}
