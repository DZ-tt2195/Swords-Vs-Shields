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
        otherPlayer.HealthRPC(-4, logged);

        List<MiniCardDisplay> availableTroops = otherPlayer.AliveTroops();
        if (availableTroops.Count == 0)
            Log.inst.AddMyText(false, "Card_Failed", "", thisCard.name, "", logged);
        else
            MakeDecision.inst.ChooseDisplayOnScreen(availableTroops, "Target_Instruction", player.name, thisCard.name, "", Protected, true);

        void Protected(Card card)
        {
            card.ProtectRPC(otherPlayer, 0, logged);
            card.ProtectRPC(otherPlayer, 1, logged);
        }
    }
}
