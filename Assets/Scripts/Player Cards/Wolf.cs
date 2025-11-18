using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Wolf : CardType
{
    public Wolf(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 3)
            return AbilityType.Defend;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-3, logged);
        Log.inst.NewDecisionContainer(() => ChooseProtect(player, thisCard, logged), logged);
    }

    void ChooseProtect(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> availableTroops = player.AliveTroops().Where(eachCard => eachCard.card.GetHealth() <= thisCard.GetHealth()).ToList();
        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}", Protect, true);
        }
        void Protect(Card card)
        {
            card.ProtectRPC(0, logged);
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
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{otherPlayer.name}", Attack, true);
        }

        void Attack(Card card)
        {
            card.HealthRPC(otherPlayer, - 3, logged);
        }
    }
}
