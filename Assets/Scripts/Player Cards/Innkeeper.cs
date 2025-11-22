using System.Collections.Generic;
using UnityEngine;

public class Innkeeper : CardType
{
    public Innkeeper(CardData dataFile) : base(dataFile)
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
        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}", Protect, true);
        }
        void Protect(Card card)
        {
            card.HealthRPC(player, 4, logged);
            card.StunRPC(1, logged);
        }
    }
}
