using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Demon : CardType
{
    public Demon(CardData dataFile) : base(dataFile)
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
        otherPlayer.HealthRPC(-4, logged);
        otherPlayer.NextRoundAction(-1);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        foreach (MiniCardDisplay display in availableTroops)
        {
            if (display.card != thisCard)
                display.card.HealthRPC(player, -1, logged);
        }
    }
}
