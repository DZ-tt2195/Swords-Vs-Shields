using System.Collections.Generic;
using UnityEngine;

public class Trickster : CardType
{
    Player otherPlayer;

    public Trickster(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        if (player.GetSword() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        otherPlayer.HealthRPC(-4, logged);
        otherPlayer.NextRoundSword(1);
    }
}
