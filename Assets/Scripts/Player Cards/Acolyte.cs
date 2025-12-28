using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Acolyte : CardType
{
    public Acolyte(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-2, logged);
        MakeDecision.inst.ChooseTextButton(new() { 
            new("Pick_Player", player.name, thisCard.name, "", HealPlayer) 
            }, $"Choose_One", player.name, thisCard.name, "", false);

        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose_One", player.name, thisCard.name, "", HealCard, false);

        void HealCard(Card card)
        {
            card.HealthRPC(player, 2, logged);
        }

        void HealPlayer()
        {
            player.HealthRPC(2, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(1, logged);
    }
}
