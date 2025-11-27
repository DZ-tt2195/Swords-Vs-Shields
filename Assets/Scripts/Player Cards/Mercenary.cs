using System.Collections.Generic;
using UnityEngine;

public class Mercenary : CardType
{
    public Mercenary(CardData dataFile) : base(dataFile)
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
        player.ActionRPC(-1, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        MakeDecision.inst.ChooseTextButton(new() { new($"Pick Player-Player-{otherPlayer.name}", AttackPlayer) }, "Choose One-Card-{thisCard.name}", false);

        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
        if (availableTroops.Count >= 1)
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose One-Card-{thisCard.name}", AttackCard, false);

        void AttackCard(Card card)
        {
            card.HealthRPC(otherPlayer, -4, logged);
        }

        void AttackPlayer()
        {
            otherPlayer.HealthRPC(-4, logged);
        }
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        player.NextRoundSword(-1);
        player.NextRoundShield(-1);
    }
}
