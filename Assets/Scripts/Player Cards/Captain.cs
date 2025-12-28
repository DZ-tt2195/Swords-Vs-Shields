using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Captain : CardType
{
    public Captain(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetShield() >= 3)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.ShieldRPC(-3, logged);
        int damage = -1*player.AliveTroops().Where(display => IsDefend(display.card.thisCard.dataFile)).Count();

        bool IsDefend(CardData card)
        {
            return (card.typeOne == AbilityType.Defend || card.typeTwo == AbilityType.Defend);
        }

        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);

        if (damage == 0)
        {
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        }
        else
        {
        MakeDecision.inst.ChooseTextButton(new() { 
            new("Pick_Player", player.name, thisCard.name, "", AttackPlayer) 
            }, $"Choose_One", player.name, thisCard.name, "", false);

            List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
            if (availableTroops.Count >= 1)
                MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose_One", player.name, thisCard.name, "", AttackCard, false);

            void AttackCard(Card card)
            {
                card.HealthRPC(otherPlayer, -damage, logged);
            }

            void AttackPlayer()
            {
                otherPlayer.HealthRPC(-damage, logged);
            }
        }
    }
}
