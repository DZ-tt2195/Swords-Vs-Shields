using System.Collections.Generic;
using UnityEngine;

public class Recruiter : CardType
{
    public Recruiter(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ActionRPC(1, logged);
    }

    protected override AbilityType CanUseAbiltyTwo(Player player, Card thisCard)
    {
        if (player.GetHand().Count >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        DiscardEffect(player, thisCard, logged, 0);
    }

    void DiscardEffect(Player player, Card thisCard, int logged, int counter)
    {
        List<Card> handCards = player.GetHand();
        MakeDecision.inst.ChooseCardOnScreen(handCards, $"Discard Instruction-Card-{thisCard.name}", Discard);

        void Discard(Card card)
        {
            player.DiscardRPC(card, logged);
            if (counter == 0)
                Log.inst.NewDecisionContainer(() => DiscardEffect(player,thisCard, logged, counter), logged);
            else
                CreateGame.inst.OtherPlayer(player.myPosition).HealthRPC(-1 * player.GetTroops().Count, logged);
        }
    }

}
