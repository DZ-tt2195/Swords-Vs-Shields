using UnityEngine;

public class Acolyte : CardType
{
    public Acolyte(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-2, logged);
        player.HealthRPC(2, logged);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(1, logged);
    }
}
