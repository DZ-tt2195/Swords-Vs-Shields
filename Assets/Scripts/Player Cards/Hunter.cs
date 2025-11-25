using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hunter : CardType
{
    public Hunter(CardData dataFile) : base(dataFile)
    {
    }

    protected override void DoAbilityOne(Player player, Card thisCard, int logged)
    {
        Player otherPlayer = CreateGame.inst.OtherPlayer(player.myPosition);
        otherPlayer.HealthRPC(-3, logged);

        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
        if (availableTroops.Count == 0)
            Log.inst.AddMyText($"Card Failed-Card-{thisCard.name}", false, logged);
        else
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, $"Target Instruction-Player-{otherPlayer.name}", Attack, true);

        void Attack(Card card)
        {
            card.StunRPC(0, logged);
            card.StunRPC(1, logged);
        }
    }
}
