using UnityEngine;
using System.Collections.Generic;

public class Dragon : CardType
{
    public Dragon(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (TurnManager.inst.GetInt(PlayerProp.Sword, player) >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-4, logged);
        Player otherPlayer = PlayerCreator.inst.OtherPlayer(player.myPosition);
        List<Card> otherCards = TurnManager.inst.GetCardList(PlayerProp.MyTroops, otherPlayer);
        foreach (Card card in otherCards)
            card.HealthRPC(otherPlayer, -2, logged);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = TurnManager.inst.GetCardList(PlayerProp.MyHand, player);
        player.ChooseCardOnScreen(handCards, Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
        }
    }
}
