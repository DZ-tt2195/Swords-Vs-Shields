using UnityEngine;

public class Researcher : CardType
{
    public Researcher(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.NextRoundShield(-1);
        player.NextRoundSword(-1);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.DrawCardRPC(2, logged);
    }
}
