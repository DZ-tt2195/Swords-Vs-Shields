using System.Collections.Generic;
using UnityEngine;

public class Golem : CardType
{
    public Golem(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetHealth() <= 10)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        MakeDecision.inst.ChooseTextButton(new() { 
            new("Pick_Player", player.name, thisCard.name, "", AttackPlayer) 
            }, $"Choose_One", player.name, thisCard.name, "", false);

        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose_One", player.name, thisCard.name, "", AttackCard, false);

        void AttackCard(Card card)
        {
            card.HealthRPC(otherPlayer, -3, logged);
        }

        void AttackPlayer()
        {
            otherPlayer.HealthRPC(-3, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.HealthRPC(-2, logged);
    }
}
