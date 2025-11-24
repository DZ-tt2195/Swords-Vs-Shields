using System.Collections.Generic;
using UnityEngine;

public class Berserker : CardType
{
    public Berserker(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        MakeDecision.inst.ChooseTextButton(new() { new($"Pick Player-Player-{otherPlayer.name}", AttackPlayer) }, "Choose One");

        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose One", AttackCard, false);

        void AttackCard(Card card)
        {
            card.HealthRPC(otherPlayer, -3, logged);
        }

        void AttackPlayer()
        {
            otherPlayer.HealthRPC(-3, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.NextRoundAction(-1);
    }
}
