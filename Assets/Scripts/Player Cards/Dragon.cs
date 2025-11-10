using UnityEngine;
using System.Collections.Generic;

public class Dragon : CardType
{
    Player otherPlayer;
    List<MiniCardDisplay> otherCards = new();

    public Dragon(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        otherPlayer = PlayerCreator.inst.OtherPlayer(player.myPosition);
        otherCards = otherPlayer.AliveTroops();

        if (otherCards.Count >= 1 && player.GetSword() >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-4, logged);
        foreach (MiniCardDisplay display in otherCards)
            display.card.HealthRPC(otherPlayer, -2, logged);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        TurnManager.inst.Instructions(-1, "Discard Instruction");
        player.ChooseCardOnScreen(handCards, Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
        }
    }
}
