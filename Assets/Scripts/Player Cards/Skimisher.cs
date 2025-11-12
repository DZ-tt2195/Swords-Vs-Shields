using UnityEngine;
using System.Collections.Generic;

public class Skirmisher : CardType
{
    public Skirmisher(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        CreateGame.inst.OtherPlayer(player.myPosition).HealthRPC(-2, logged);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(1, logged);
    }
}
