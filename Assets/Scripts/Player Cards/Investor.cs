using System.Collections.Generic;
using UnityEngine;

public class Investor : CardType
{
    public Investor(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (thisCard.GetHealth() >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        thisCard.HealthRPC(player, -2, logged);
        player.HealthRPC(3, logged);
    }
}
