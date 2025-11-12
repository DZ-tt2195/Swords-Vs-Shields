using UnityEngine;

public class Guardian : CardType
{
    public Guardian(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 4)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-4, logged);
        player.HealthRPC(player.GetTroops().Count, logged);
    }
}
