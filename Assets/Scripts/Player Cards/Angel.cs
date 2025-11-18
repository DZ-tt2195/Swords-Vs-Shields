using UnityEngine;

public class Angel : CardType
{
    public Angel(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-2, logged);
        foreach (MiniCardDisplay display in player.AliveTroops())
            display.card.HealthRPC(player, 1, logged);
    }
}
