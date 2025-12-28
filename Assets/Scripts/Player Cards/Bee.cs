using System.Collections.Generic;
using UnityEngine;

public class Bee : CardType
{
    public Bee(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        Log.inst.NewDecisionContainer(() => ChooseAttack(player, thisCard, logged), logged);
    }

    void ChooseAttack(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
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
            card.HealthRPC(otherPlayer, -1, logged);
            card.StunRPC(otherPlayer, 1, logged);
        }
    }
}
