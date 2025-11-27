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
        int damage = -1 * player.AliveTroops().Where(display => IsAttack(display.card.thisCard.dataFile)).Count();

        bool IsAttack(CardData card)
        {
            return (card.typeOne == AbilityType.Attack || card.typeTwo == AbilityType.Attack);
        }

        if (damage == 0)
        {
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        }
        else
        {
            MakeDecision.inst.ChooseTextButton(new() { new($"Pick Player-Player-{player.name}", HealPlayer) }, $"Choose One-Card-{thisCard.name}", false);

            List<MiniCardDisplay> availableTroops = player.AliveTroops();
            if (availableTroops.Count >= 1)
                MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Choose One-Card-{thisCard.name}", HealCard, false);

            void HealCard(Card card)
            {
                card.HealthRPC(player, -damage, logged);
            }

            void HealPlayer()
            {
                player.HealthRPC(-damage, logged);
            }
        }
    }
}
