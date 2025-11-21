using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Vassal : CardType
{
    public Vassal(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetTroops().Count >= 2)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> availableTroops = player.AliveTroops().Where(display => display.card != thisCard).ToList();
        MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}", HurtCard, true);

        void HurtCard(Card card)
        {
            card.HealthRPC(player, -3, logged);
            Log.inst.NewDecisionContainer(() => PartTwo(player, thisCard, logged), logged);
        }
    }

    void PartTwo(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}", ProtectCard, true);

        void ProtectCard(Card card)
        {
            card.ProtectRPC(0, logged);
        }
    }
}
