using UnityEngine;
using System.Collections.Generic;

public class Boat : CardType
{
    public Boat(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 1)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-1, logged);
        PlayerCreator.inst.OtherPlayer(player.myPosition).HealthRPC(-2, logged);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(1, logged);
    }
}
