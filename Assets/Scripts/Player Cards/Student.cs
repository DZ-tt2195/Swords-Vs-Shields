using System.Collections.Generic;
using UnityEngine;

public class Student : CardType
{
    public Student(CardData dataFile) : base(dataFile)
    {
    }

    List<MiniCardDisplay> FindAbilities(Player player, Card toIgnore, AbilityType toFind)
    {
        List<MiniCardDisplay> toFindCards = new();
        foreach (MiniCardDisplay display in player.AliveTroops())
        {
            Card card = display.card;
            if (card == toIgnore || !card.CanUseAbility())
                continue;
            if (card.thisCard.CanUseAbiltyOne(player, card) == toFind)
                toFindCards.Add(display);
            if (card.thisCard.CanUseAbiltyTwo(player, card) == toFind)
                toFindCards.Add(display);
        }
        return toFindCards;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> defendAbilities = FindAbilities(player, thisCard, AbilityType.Defend);
        if (defendAbilities.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(defendAbilities, $"Target Instruction-Player-{player.name}", ChooseToUse, true);
        else
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);

        void ChooseToUse(Card card)
        {
            Log.inst.AddMyText($"Resolve Card-Player-{player.name}-Card-{card.name}", false, logged);
            if (card.thisCard.CanUseAbiltyOne(player, card) == AbilityType.Defend)
                Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityOne(player, card, logged+1), logged+1);
            if (card.thisCard.CanUseAbiltyTwo(player, card) == AbilityType.Defend)
                Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityTwo(player, card, logged+1), logged+1);
        }
    }

    public override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> attackAbilities = FindAbilities(player, thisCard, AbilityType.Attack);
        if (attackAbilities.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(attackAbilities, $"Target Instruction-Player-{player.name}", ChooseToUse, true);
        else
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);

        void ChooseToUse(Card card)
        {
            Log.inst.AddMyText($"Resolve Card-Player-{player.name}-Card-{card.name}", false, logged);
            if (card.thisCard.CanUseAbiltyOne(player, card) == AbilityType.Attack)
                Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityOne(player, card, logged + 1), logged + 1);
            if (card.thisCard.CanUseAbiltyTwo(player, card) == AbilityType.Attack)
                Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityTwo(player, card, logged + 1), logged + 1);
        }
    }
}
