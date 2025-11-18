using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ninja : CardType
{
    public Ninja(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 1)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-1, logged);
        Log.inst.NewDecisionContainer(() => ChooseAttack(player, thisCard, logged), logged);
    }

    void ChooseAttack(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops().Where(display => display.card.GetHealth() <= 3).ToList();

        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{otherPlayer.name}", Attack, true);
        }

        void Attack(Card card)
        {
            card.HealthRPC(otherPlayer, -3, logged);
        }
    }
}
