using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Balancer : CardType
{
    public Balancer(CardData dataFile) : base(dataFile)
    {
    }

    void ChooseCard(Player chooseFrom, Card thisCard, int logged, int healthAmount)
    {
        List<MiniCardDisplay> availableTroops = chooseFrom.AliveTroops();
        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{chooseFrom.name}-Card-{thisCard.name}", Target, true);
        }

        void Target(Card card)
        {
            card.HealthRPC(chooseFrom, healthAmount, logged);
        }

    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        Log.inst.NewDecisionContainer(() => ChooseCard(player, thisCard, logged, 3), logged);
        Log.inst.NewDecisionContainer(() => ChooseCard(player, thisCard, logged, -3), logged);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        Log.inst.NewDecisionContainer(() => ChooseCard(otherPlayer, thisCard, logged, 3), logged);
        Log.inst.NewDecisionContainer(() => ChooseCard(otherPlayer, thisCard, logged, -3), logged);
    }
}
