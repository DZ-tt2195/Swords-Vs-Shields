using UnityEngine;
using System.Collections.Generic;

public class Dragon : CardType
{
    public Dragon(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 6)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-6, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        foreach (MiniCardDisplay display in otherPlayer.AliveTroops())
            display.card.HealthRPC(otherPlayer, -2, logged);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.Instructions("Discard Instruction");
        MakeDecision.inst.ChooseCardOnScreen(handCards, Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
        }
    }
}
