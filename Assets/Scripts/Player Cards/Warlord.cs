using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Warlord : CardType
{
    public Warlord(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetHand().Count >= 1)
            return AbilityType.Attack;
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
            Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
            otherPlayer.HealthRPC(-2, logged);
            otherPlayer.NextRoundShield(-1);
            otherPlayer.NextRoundSword(-1);
        }
    }
}
