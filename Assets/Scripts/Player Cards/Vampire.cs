using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vampire : CardType
{
    public Vampire(CardData dataFile) : base(dataFile)
    {
    }

    protected override AbilityType CanUseAbiltyOne(Player player, Card thisCard)
    {
        if (player.GetSword() >= 5)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-5, logged);
        CreateGame.inst.OtherPlayer(player.myPosition).HealthRPC(-6, logged);
    }

    protected override void DoAbilityTwo(Player player, Card thisCard, int logged)
    {
        List<MiniCardDisplay> availableTroops = player.AliveTroops();
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{player.name}", Damage, true);

        void Damage(Card card)
        {
            card.HealthRPC(player, -3, logged);
        }
    }
}
