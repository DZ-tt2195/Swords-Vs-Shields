using UnityEngine;

public class Blacksmith : CardType
{
    public Blacksmith(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 2 && player.GetShield() >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        player.ShieldRPC(-2, logged);
        player.DrawCardRPC(1, logged);
        player.ActionRPC(1, logged);
    }
}
