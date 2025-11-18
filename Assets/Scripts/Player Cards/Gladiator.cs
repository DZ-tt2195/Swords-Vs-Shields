using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Gladiator : CardType
{
    public Gladiator(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetHand().Count >= 1)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, "Discard Instruction", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            Log.inst.NewDecisionContainer(() => ChooseAttack(player, thisCard, logged), logged);
        }
    }

    void ChooseAttack(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops().Where(display => IsAttack(display.card.thisCard.dataFile)).ToList();

        bool IsAttack(CardData card)
        {
            return (card.typeOne == AbilityType.Attack || card.typeTwo == AbilityType.Attack);
        }

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
            card.HealthRPC(otherPlayer, -4, logged);
        }
    }
}
