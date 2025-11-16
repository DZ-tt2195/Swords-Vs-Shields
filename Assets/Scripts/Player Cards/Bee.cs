using System.Collections.Generic;
using UnityEngine;

public class Bee : CardType
{
    public Bee(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
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
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.Instructions($"Target Instruction-Player-{player.name}");
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, Attack, true);
        }

        void Attack(Card card)
        {
            card.HealthRPC(otherPlayer, -2, logged);
            card.StunRPC(1, logged);
        }
    }
}
