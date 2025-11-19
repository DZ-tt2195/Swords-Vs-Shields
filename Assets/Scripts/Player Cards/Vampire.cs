using System;
using System.Collections.Generic;
using UnityEngine;

public class Vampire : CardType
{
    public Vampire(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 3)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-3, logged);
        player.HealthRPC(3, logged);
    }

    protected override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetShield() >= 3)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-3, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        otherPlayer.HealthRPC(-3, logged);
    }
}
