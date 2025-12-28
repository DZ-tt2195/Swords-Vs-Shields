using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Barbarian : CardType
{
    public Barbarian(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetAction() >= 1)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(-1, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops().Where(display => display.card.GetHealth() >= 5).ToList();

        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, "Target_Instruction", otherPlayer.name, thisCard.name, "", Attack, true);
        }

        void Attack(Card card)
        {
            card.HealthRPC(otherPlayer, -4, logged);
        }
    }
}
