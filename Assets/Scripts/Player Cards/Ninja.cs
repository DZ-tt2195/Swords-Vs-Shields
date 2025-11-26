using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ninja : CardType
{
    public Ninja(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
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
            card.HealthRPC(otherPlayer, -2, logged);
        }
    }
}
