using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Guardian : CardType
{
    public Guardian(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 1)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-1, logged);
        MakeDecision.inst.ChooseTextButton(new() { new($"Pick Player-Player-{player.name}", HealPlayer) }, $"Choose One-Card-{thisCard.name}", false);

        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose One-Card-{thisCard.name}", HealCard, false);

        void HealCard(Card card)
        {
            card.HealthRPC(player, 1, logged);
        }

        void HealPlayer()
        {
            player.HealthRPC(1, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}-Card-{thisCard.name}", Protect, true);
        }
        void Protect(Card card)
        {
            card.ProtectRPC(0, logged);
        }
    }
}
