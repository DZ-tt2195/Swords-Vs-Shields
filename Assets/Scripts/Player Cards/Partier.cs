using UnityEngine;

public class Partier : CardType
{
    public Partier(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 5)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-5, logged);
        player.HealthRPC(player.GetTroops().Count, logged);
    }

    protected override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetSword() >= 5)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-5, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        otherPlayer.HealthRPC(-1 * otherPlayer.GetTroops().Count, logged);
    }
}
