using System.Collections.Generic;
using UnityEngine;

public class Bee : CardType
{
    Player otherPlayer;
    List<MiniCardDisplay> otherCards = new();

    public Bee(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        otherPlayer = PlayerCreator.inst.OtherPlayer(player.myPosition);
        otherCards = otherPlayer.AliveTroops();

        if (otherCards.Count >= 1 && player.GetSword() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    public override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        Log.inst.NewDecisionContainer(() => ChooseAttack(player, logged), logged);
    }

    void ChooseAttack(Player player, int logged)
    {
        TurnManager.inst.Instructions(-1, $"Target Instruction-Player-{otherPlayer.name}");
        player.ChooseDisplayOnScreen(otherCards, DamageAndStun);

        void DamageAndStun(Card card)
        {
            card.HealthRPC(otherPlayer, -2, logged);
            card.StunRPC(1, logged);
        }
    }
}
