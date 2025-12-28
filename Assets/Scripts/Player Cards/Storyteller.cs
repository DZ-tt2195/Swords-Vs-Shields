using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Storyteller : CardType
{
    public Storyteller(CardData dataFile) : base(dataFile)
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
        int healing = -1 * player.AliveTroops().Where(display => IsAttack(display.card.thisCard.dataFile)).Count();

        bool IsAttack(CardData card)
        {
            return (card.typeOne == AbilityType.Attack || card.typeTwo == AbilityType.Attack);
        }

        if (healing == 0)
        {
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        }
        else
        {
            MakeDecision.inst.ChooseTextButton(new() { 
            new("Pick_Player", player.name, thisCard.name, "", HealPlayer) 
            }, $"Choose_One", player.name, thisCard.name, "", false);

            List<MiniCardDisplay> availableTroops = player.AliveTroops();
            if (availableTroops.Count >= 1)
                MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose_One", player.name, thisCard.name, "", HealCard, false);

            void HealCard(Card card)
            {
                card.HealthRPC(player, healing, logged);
            }

            void HealPlayer()
            {
                player.HealthRPC(healing, logged);
            }
        }
    }
}
