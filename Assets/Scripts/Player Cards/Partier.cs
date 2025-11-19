using UnityEngine;

public class Partier : CardType
{
    public Partier(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 4)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-4, logged);
        player.HealthRPC(player.GetTroops().Count, logged);
    }
}
