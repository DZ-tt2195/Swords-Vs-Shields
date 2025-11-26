using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mob : CardType
{
    public Mob(CardData dataFile) : base(dataFile)
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
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, $"Discard Instruction-Card-{thisCard.name}", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            Log.inst.NewDecisionContainer(() => AttackFoe(player, thisCard, logged), logged);
        }
    }

    void AttackFoe(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops().Where(display => display.card.GetHealth() <= 2).ToList();

        if (availableTroops.Count == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{otherPlayer.name}-Card-{thisCard.name}", Attack, true);
        }

        void Attack(Card card)
        {
            card.HealthRPC(otherPlayer, -1 * thisCard.GetHealth(), logged);
        }
    }
}
