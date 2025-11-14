using System.Collections.Generic;
using UnityEngine;

public class Minstrel : CardType
{
    public Minstrel(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetHand().Count >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.Instructions("Discard Instruction");
        MakeDecision.inst.ChooseCardOnScreen(handCards, Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            player.NextRoundAction(1);
        }
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(1, logged);
    }
}
