using System.Collections.Generic;
using UnityEngine;

public class Leprechaun : CardType
{
    public Leprechaun(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        if (player.GetTroops().Count == 7)
        {
            player.SwordRPC(2, logged);
            player.ShieldRPC(2, logged);
        }
        else
        {
            player.HealthRPC(-2, logged);
        }
    }
}
