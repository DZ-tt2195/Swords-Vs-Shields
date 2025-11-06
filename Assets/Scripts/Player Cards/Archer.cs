using UnityEngine;

public class Archer : CardType
{
    public Archer(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (TurnManager.inst.GetInt(PlayerProp.Sword, player) >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-1, logged);
        player.DrawCardRPC(1, logged);
    }

    public override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (TurnManager.inst.GetInt(PlayerProp.Sword, player) >= 3)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-3, logged);
        int playerHand = TurnManager.inst.GetCardList(PlayerProp.MyHand, player).Count;
        PlayerCreator.inst.OtherPlayer(player.myPosition).HealthRPC(-1*playerHand, logged);
    }
}
