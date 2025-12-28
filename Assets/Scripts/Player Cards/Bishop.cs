using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Bishop : CardType
{
    public Bishop(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 3)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-3, logged);
        List<MiniCardDisplay> availableTroops = player.AliveTroops().Where(display => display.card.GetHealth() <= 3).ToList();

        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, "Target_Instruction", player.name, thisCard.name, "", Raise, true);
        }

        void Raise(Card card)
        {
            card.HealthRPC(player, 4-card.GetHealth(), logged);
        }
    }
}
