using UnityEngine;
using System.Collections.Generic;

public class Dragon : CardType
{
    public Dragon(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 6)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-6, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        foreach (MiniCardDisplay display in otherPlayer.AliveTroops())
            display.card.HealthRPC(otherPlayer, -2, logged);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, "Discard_Instruction", player.name, thisCard.name, "", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
        }
    }
}
