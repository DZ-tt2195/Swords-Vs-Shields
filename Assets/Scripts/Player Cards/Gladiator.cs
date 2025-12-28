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
        if (player.GetSword() >= 2)
            return AbilityType.Attack;
        else
            return AbilityType.None;
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        player.SwordRPC(-2, logged);
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops().Where(display => IsAttack(display.card.thisCard.dataFile)).ToList();

        if (availableTroops.Count == 0)
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        else
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, "Target_Instruction", player.name, thisCard.name, "", Attack, true);

        bool IsAttack(CardData card)
        {
            return (card.typeOne == AbilityType.Attack || card.typeTwo == AbilityType.Attack);
        }

        void Attack(Card card)
        {
            card.HealthRPC(otherPlayer, -3, logged);
        }
    }
}
