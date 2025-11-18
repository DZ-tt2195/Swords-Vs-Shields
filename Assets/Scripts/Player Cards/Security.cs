using UnityEngine;

public class Security : CardType
{
    public Security(CardData dataFile) : base(dataFile)
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
        player.NextRoundShield(2);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.HealthRPC(3, logged);
    }
}
