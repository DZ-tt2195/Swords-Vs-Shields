using UnityEngine;

public class Angel : CardType
{
    public Angel(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.HealthRPC(-2, logged);
        foreach (MiniCardDisplay display in player.AliveTroops())
            display.card.HealthRPC(player, 1, logged);
    }
}
