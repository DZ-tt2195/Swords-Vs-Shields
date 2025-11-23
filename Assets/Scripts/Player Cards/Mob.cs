using UnityEngine;

public class Mob : CardType
{
    public Mob(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        thisCard.HealthRPC(player, 1, logged);
    }

    protected override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetAction() >= 1)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(-1, logged);
        CreateGame.inst.OtherPlayer(player.myPosition).HealthRPC(-1 * thisCard.GetHealth(), logged);
    }
}
