using UnityEngine;

public class Angel : CardType
{
    public Angel(CardData dataFile) : base(dataFile)
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
        foreach (Card card in player.GetTroops())
            card.HealthRPC(player, 1, logged);
    }
}
