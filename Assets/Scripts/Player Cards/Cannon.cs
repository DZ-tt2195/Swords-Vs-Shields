using UnityEngine;

public class Cannon : CardType
{
    Player otherPlayer;

    public Cannon(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        if (player.GetShield() >= 4)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-4, logged);
        otherPlayer.HealthRPC(-6, logged);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        thisCard.StunRPC(0, logged);
    }
}
