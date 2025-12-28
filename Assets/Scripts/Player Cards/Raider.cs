using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Raider : CardType
{
    public Raider(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-2, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops().Where(display => IsDefend(display.card.thisCard.dataFile)).ToList();

        bool IsDefend(CardData card)
        {
            return (card.typeOne == AbilityType.Defend || card.typeTwo == AbilityType.Defend);
        }

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
            card.HealthRPC(otherPlayer, -3, logged);
        }
    }
}
