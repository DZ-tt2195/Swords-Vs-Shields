using System.Collections.Generic;
using UnityEngine;

public class Royalty : CardType
{
    public Royalty(CardData dataFile) : base(dataFile)
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
        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, "Target_Instruction", player.name, thisCard.name, "", Protect, true);
        }
        void Protect(Card card)
        {
            player.HealthRPC(Mathf.FloorToInt(card.GetHealth() / 2), logged);
        }
    }
}
