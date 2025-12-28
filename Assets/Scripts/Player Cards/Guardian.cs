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
        MakeDecision.inst.ChooseTextButton(new() { 
            new("Pick_Player", player.name, thisCard.name, "", HealPlayer) 
            }, $"Choose_One", player.name, thisCard.name, "", false);

        List<MiniCardDisplay> availableTroops = player.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose_One", player.name, thisCard.name, "", HealCard, false);

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
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, "Target_Instruction", player.name, thisCard.name, "", Protect, true);
        }
        void Protect(Card card)
        {
            card.ProtectRPC(player, 0, logged);
        }
    }
}
