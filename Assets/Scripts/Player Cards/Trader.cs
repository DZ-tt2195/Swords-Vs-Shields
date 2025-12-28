using System.Collections.Generic;
using UnityEngine;

public class Trader : CardType
{
    public Trader(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetAction() >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(-1, logged);
        MakeDecision.inst.ChooseTextButton(new() { new("Sword", "", "", "", Sword), new("Shield", "", "", "", Shield) }, "Choose_One_Instruction", "", "", "");

        void Sword()
        {
            player.SwordRPC(2, logged);
        }
        void Shield()
        {
            player.ShieldRPC(2, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, "Discard_Instruction", player.name, thisCard.name, "", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            player.DrawCardRPC(1, logged);
        }
    }
}
