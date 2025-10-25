using System.Text.RegularExpressions;
using UnityEngine;
using System;

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

    public virtual void WhenPlayThis(Player player, Card thisCard, int logged)
    {

    }

    public virtual void WhenPlayOther(Player player, Card thisCard, Card playedCard, int logged)
    {

    }

    public virtual void WhenThisMove(Player player, Card thisCard, int logged)
    {

    }

    public virtual void WhenOtherMove(Player player, Card thisCard, Card movedCard, int logged)
    {

    }

    public virtual void WhenVisit(Player player, Card thisCard, int logged)
    {

    }

    public virtual void WhenBoxOnThis(Player player, Card thisCard, int num, int logged)
    {

    }
}
