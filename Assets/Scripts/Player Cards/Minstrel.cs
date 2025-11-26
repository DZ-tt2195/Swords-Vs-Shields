using System.Collections.Generic;
using UnityEngine;

public class Minstrel : CardType
{
    public Minstrel(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetHand().Count >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        DiscardEffect(player, thisCard, 0, logged);
    }

    void DiscardEffect(Player player, Card thisCard, int logged, int counter)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, $"Discard Instruction-Card-{thisCard.name}", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            if (counter < 1)
                Log.inst.NewDecisionContainer(() => DiscardEffect(player, thisCard, logged, counter), logged);
            else
                player.NextRoundAction(1);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(1, logged);
    }
}
