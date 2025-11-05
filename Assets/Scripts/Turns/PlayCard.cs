using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayCard", menuName = "ScriptableObjects/PlayCard")]
public class PlayCard : Turn
{
    public override void ForMaster()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        Log.inst.MasterText($"Play Card-Num-{currentRound}");
        Log.inst.MasterText("Blank");
    }

    public override void ForPlayer(Player player)
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        player.DrawCardRPC(currentRound == 1 ? 4 : 2, 0);
        player.ActionRPC(2, -1);
        player.GreenCoinRPC(currentRound - TurnManager.inst.GetInt(PlayerProp.GreenCoin, player), -1);
        player.RedCoinRPC(currentRound - TurnManager.inst.GetInt(PlayerProp.RedCoin, player), -1);
        Log.inst.NewDecisionContainer(() => PlayLoop(player), 0);
    }

    void PlayLoop(Player player)
    {
        if (TurnManager.inst.GetInt(PlayerProp.Action, player) >= 1)
        {
            player.ChooseButtonInPopup(new() { new("Decline") }, "Play Card Instruction", new(0, 325), false);
            List<Card> myHand = TurnManager.inst.GetCardList(PlayerProp.MyHand, player);
            player.ChooseCardOnScreen(myHand, ChooseToPlay, false);

            void ChooseToPlay(Card card)
            {
                Log.inst.AddMyText($"Play Troop-Player-{player.name}-Card-{card.name}", true);
                player.ActionRPC(-1, -1);

                card.HealthRPC(player, card.thisCard.dataFile.startingHealth, -1);
                Log.inst.NewRollback(() => HandToPlay(player, card));

                if (card.thisCard.CanUseAbiltyOne(player, card) == AbilityType.PlayEffect)
                    Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityOne(player, card, 1), 1);
                if (card.thisCard.CanUseAbiltyTwo(player, card) == AbilityType.PlayEffect)
                    Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityTwo(player, card, 1), 1);

                Log.inst.NewDecisionContainer(() => PlayLoop(player), 0);
            }
        }
    }

    void HandToPlay(Player player, Card cardToPlay)
    {
        List<Card> myHand = TurnManager.inst.GetCardList(PlayerProp.MyHand, player);
        List<Card> myTroops = TurnManager.inst.GetCardList(PlayerProp.MyTroops, player);

        if (!Log.inst.forward)
        {
            myHand.Add(cardToPlay);
            myTroops.Remove(cardToPlay);
        }
        else
        {
            myHand.Remove(cardToPlay);
            myTroops.Add(cardToPlay);
        }
        TurnManager.inst.WillChangePlayerProperty(PlayerProp.MyHand, TurnManager.inst.ConvertCardList(myHand));
        TurnManager.inst.WillChangePlayerProperty(PlayerProp.MyTroops, TurnManager.inst.ConvertCardList(myTroops));
    }
}
