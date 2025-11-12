using UnityEngine;

public class Archer : CardType
{
    public Archer(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        player.DrawCardRPC(1, logged);
    }

    public override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetSword() >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-4, logged);
        int playerHand = player.GetHand().Count;
        CreateGame.inst.OtherPlayer(player.myPosition).HealthRPC(-1*playerHand, logged);
    }
}
