using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class Coven : CardType
{
    public Coven(CardData dataFile) : base(dataFile)
    {
    }

    void ChooseAnother(Player toChooseFrom, Card thisCard, HashSet<Card> toExclude, Action<Card> action, int logged)
    {
        List<MiniCardDisplay> canChoose = new();
        foreach (MiniCardDisplay display in toChooseFrom.AliveTroops())
        {
            if (!toExclude.Contains(display.card))
                canChoose.Add(display);
        }

        if (canChoose.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(canChoose, $"Target Instruction-Player-{toChooseFrom.name}-Card-{thisCard.name}", Effects, true);
        else
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);

        void Effects(Card card)
        {
            action(card);
            HashSet<Card> newSet = new(toExclude);
            newSet.Add(card);
            if (newSet.Count < 3)
                Log.inst.NewDecisionContainer(() => ChooseAnother(toChooseFrom, thisCard, newSet, action, logged), logged);
        }
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
        ChooseAnother(player, thisCard, new(), Heal, logged);

        void Heal(Card card)
        {
            card.HealthRPC(player, 1, logged);
        }
    }

    protected override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetSword() >= 3)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-3, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        ChooseAnother(otherPlayer, thisCard, new(), Damage, logged);

        void Damage(Card card)
        {
            card.HealthRPC(otherPlayer, -1, logged);
        }
    }
}
