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
        otherPlayer.HealthRPC(-3, logged);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.NextRoundAction(-1);
    }
}
