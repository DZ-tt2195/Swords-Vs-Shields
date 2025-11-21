using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bureaucrat : CardType
{
    public Bureaucrat(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-4, logged);

        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        otherPlayer.HealthRPC(-2, logged);
        otherPlayer.NextRoundShield(-1);
        otherPlayer.NextRoundSword(-1);
    }
}
