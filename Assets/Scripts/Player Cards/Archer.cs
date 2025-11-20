using UnityEngine;

public class Archer : CardType
{
    public Archer(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetAction() >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(-1, logged);
        player.DrawCardRPC(1, logged);
    }

    protected override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetSword() >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-4, logged);
        int playerHand = player.GetHand().Count;
        CreateGame.inst.OtherPlayer(player.myPosition).HealthRPC(-1*playerHand, logged);
    }
}
