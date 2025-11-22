using System.Collections.Generic;
using UnityEngine;

public class Cannon : CardType
{
    Player otherPlayer;

    public Cannon(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        if (player.GetShield() >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-4, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        MakeDecision.inst.ChooseTextButton(new() { new($"Pick Player-Player-{otherPlayer.name}", AttackPlayer) }, "Choose One");

        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose One", AttackCard, false);

        void AttackCard(Card card)
        {
            card.HealthRPC(otherPlayer, -6, logged);
        }

        void AttackPlayer()
        {
            otherPlayer.HealthRPC(-6, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        thisCard.StunRPC(0, logged);
    }
}
