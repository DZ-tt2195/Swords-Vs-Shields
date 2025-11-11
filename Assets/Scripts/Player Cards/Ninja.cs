using System.Collections.Generic;
using UnityEngine;

public class Ninja : CardType
{
    Player otherPlayer;
    List<MiniCardDisplay> otherCards = new();

    public Ninja(CardData dataFile) : base(dataFile)
    {
    }

    public override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        otherPlayer = PlayerCreator.inst.OtherPlayer(player.myPosition);
        otherCards = otherPlayer.AliveTroops();
        for (int i = otherCards.Count-1; i>= 0; i--)
        {
            if (!(otherCards[i].card.GetHealth() <= 3))
                otherCards.RemoveAt(i);
        }

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
        player.ChooseDisplayOnScreen(otherCards, Damage);

        void Damage(Card card)
        {
            card.HealthRPC(otherPlayer, -3, logged);
        }
    }
}
