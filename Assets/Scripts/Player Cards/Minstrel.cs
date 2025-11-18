using System.Collections.Generic;
using UnityEngine;

public class Minstrel : CardType
{
    public Minstrel(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetHand().Count >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, "Discard Instruction", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            player.NextRoundAction(1);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(1, logged);
    }
}
