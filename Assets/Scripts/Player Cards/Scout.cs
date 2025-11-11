using UnityEngine;

public class Scout : CardType
{
    public Scout(CardData dataFile) : base(dataFile)
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
        player.NextRoundSword(2);
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.DrawCardRPC(1, logged);
    }
}
