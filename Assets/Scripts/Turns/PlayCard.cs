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
        player.DrawCardRPC(currentRound == 1 ? 4 : 2);
        player.ActionRPC(2);

        player.ShieldRPC(currentRound - player.GetShield());
        int nextRoundShield = TurnManager.inst.GetInt(PlayerProp.NextRoundShield, player);
        player.ShieldRPC(nextRoundShield, 1);
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.NextRoundShield, 0);

        player.SwordRPC(currentRound - player.GetSword());
        int nextRoundSword = TurnManager.inst.GetInt(PlayerProp.NextRoundSword, player);
        player.SwordRPC(nextRoundSword, 1);
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.NextRoundSword, 0);

        Log.inst.NewDecisionContainer(() => PlayLoop(player), 0);
    }

    void PlayLoop(Player player)
    {
        if (player.GetAction() >= 1)
        {
            player.ChooseButtonInPopup(new() { new("Decline") }, "Play Card Instruction", new(0, 325), false);
            List<Card> myHand = player.GetHand();
            player.ChooseCardOnScreen(myHand, ChooseToPlay, false);

            void ChooseToPlay(Card card)
            {
                Log.inst.AddMyText($"Play Troop-Player-{player.name}-Card-{card.name}", true);
                player.ActionRPC(-1, -1);

                card.HealthRPC(player, card.thisCard.dataFile.startingHealth, -1);
                Log.inst.NewRollback(() => HandToPlay(player, card));

                if (card.thisCard.CanUseAbiltyOne(player, card) == AbilityType.Play)
                    Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityOne(player, card, 1), 1);
                if (card.thisCard.CanUseAbiltyTwo(player, card) == AbilityType.Play)
                    Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityTwo(player, card, 1), 1);

                Log.inst.NewDecisionContainer(() => PlayLoop(player), 0);
            }
        }
    }

    void HandToPlay(Player player, Card cardToPlay)
    {
        List<Card> myHand = player.GetHand();
        List<Card> myTroops = player.GetTroops();

        if (!Log.inst.forward)
        {
            myHand.Add(cardToPlay);
            myTroops.Remove(cardToPlay);
        }
        else
        {
            myHand.Remove(cardToPlay);
            myTroops.Add(cardToPlay);
            cardToPlay.transform.SetParent(null);
            cardToPlay.MoveCardRPC(new(0, 10000), 0.25f, Vector3.one);
        }
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.MyHand, TurnManager.inst.ConvertCardList(myHand));
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.MyTroops, TurnManager.inst.ConvertCardList(myTroops));
    }
}
