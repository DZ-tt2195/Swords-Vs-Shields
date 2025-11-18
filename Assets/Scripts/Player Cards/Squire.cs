using UnityEngine;

public class Squire : CardType
{
    public Squire(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 3)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-3, logged);
        player.NextRoundSword(2);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.DrawCardRPC(1, logged);
    }
}
