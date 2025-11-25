using System;
using System.Collections.Generic;
using UnityEngine;

public class Farmer : CardType
{
    public Farmer(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 4)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-4, logged);
        for (int i = 0; i<2; i++)
            Log.inst.NewDecisionContainer(() => ChooseHeal(player, logged), logged);
    }

    void ChooseHeal(Player player, int logged)
    {
        MakeDecision.inst.ChooseTextButton(new() { new($"Pick Player-Player-{player.name}", HealPlayer) }, "Choose One", false);

        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose One", HealCard, false);

        void HealCard(Card card)
        {
            card.HealthRPC(player, 2, logged);
        }

        void HealPlayer()
        {
            player.HealthRPC(2, logged);
        }
    }
}
