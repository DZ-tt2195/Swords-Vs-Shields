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
        player.HealthRPC(2, logged);

        List<MiniCardDisplay> availableTroops = player.AliveTroops();

        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}-Card-{thisCard.name}", Heal, true);
        }

        void Heal(Card card)
        {
            card.HealthRPC(player, 2, logged);
        }

    }
}
