using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GreenAbilities", menuName = "ScriptableObjects/GreenAbilities")]
public class GreenAbilities : Turn
{
    public override void MasterStart()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        Log.inst.MasterText($"Use Green-Num-{currentRound}");
    }

    public override void MasterEnd()
    {
        Log.inst.MasterText($"Blank");
    }

    public override void ForPlayer(Player player)
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        player.DrawCardRPC(currentRound == 1 ? 4 : 2);

        player.ActionRPC(2);
        int nextRoundAction = TurnManager.inst.GetInt(PlayerProp.NextRoundAction, player);
        player.ActionRPC(nextRoundAction, 1);
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.NextRoundAction, 0);

        player.ShieldRPC(currentRound - player.GetShield());
        int nextRoundShield = TurnManager.inst.GetInt(PlayerProp.NextRoundShield, player);
        player.ShieldRPC(nextRoundShield, 1);
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.NextRoundShield, 0);

        player.SwordRPC(currentRound - player.GetSword());
        int nextRoundSword = TurnManager.inst.GetInt(PlayerProp.NextRoundSword, player);
        player.SwordRPC(nextRoundSword, 1);
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.NextRoundSword, 0);

        Log.inst.NewDecisionContainer(() => NextAbility(player, new()), 0);
    }

    void NextAbility(Player player, HashSet<Card> alreadyDone)
    {
        List<MiniCardDisplay> greenCards = new();

        foreach (MiniCardDisplay display in player.AliveTroops())
        {
            Card card = display.card;
            if (alreadyDone.Contains(card) || !card.CanUseAbility())
                continue;
            if (card.thisCard.HasType(AbilityType.Defend, player, card, -1))
                greenCards.Add(display);
        }

        if (greenCards.Count >= 1)
        {
            MakeDecision.inst.ChooseDisplayOnScreen(greenCards, "Use Green Instruction", ChooseToUse, false);

            void ChooseToUse(Card card)
            {
                Log.inst.AddMyText($"Resolve Card-Player-{player.name}-Card-{card.name}", false);
                card.thisCard.HasType(AbilityType.Defend, player, card, 1);

                HashSet<Card> newSet = new(alreadyDone);
                newSet.Add(card);
                Log.inst.NewDecisionContainer(() => NextAbility(player, newSet), 0);
            }
        }

        if (player.GetAction() >= 1)
        {
            if (greenCards.Count == 0)
                MakeDecision.inst.ChooseTextButton(new() { new("Done", Decline) }, "Use Green Instruction", false);

            List<Card> myHand = player.GetHand();
            MakeDecision.inst.ChooseCardOnScreen(myHand, "Use Green Instruction", ChooseToPlay, false);

            void ChooseToPlay(Card card)
            {
                Log.inst.AddMyText($"Play Card-Player-{player.name}-Card-{card.name}", true);
                player.ActionRPC(-1, -1);

                card.HealthRPC(player, card.thisCard.dataFile.startingHealth, -1);
                Log.inst.NewRollback(() => HandToPlay(player, card));
                card.thisCard.HasType(AbilityType.Play, player, card, 1);
                Log.inst.NewDecisionContainer(() => NextAbility(player, alreadyDone), 0);
            }

            void Decline()
            {
                Log.inst.AddMyText($"End Turn-Player-{player.name}", false);
            }
        }
    }

    void HandToPlay(Player player, Card cardToPlay)
    {
        List<Card> myHand = player.GetHand();
        List<Card> myTroops = player.GetTroops();
        List<string> myCardsPlayed = TurnManager.inst.GetStringList(PlayerProp.AllCardsPlayed, player);

        if (!Log.inst.forward)
        {
            myHand.Add(cardToPlay);
            myTroops.Remove(cardToPlay);
            myCardsPlayed.RemoveAt(myCardsPlayed.Count - 1);
        }
        else
        {
            myHand.Remove(cardToPlay);
            myTroops.Add(cardToPlay);
            int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
            myCardsPlayed.Add($"Played Card Info-Card-{cardToPlay.name}-Num-{currentRound}");
        }
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.MyHand, TurnManager.inst.ConvertCardList(myHand));
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.MyTroops, TurnManager.inst.ConvertCardList(myTroops));
        TurnManager.inst.WillChangePlayerProperty(player, PlayerProp.AllCardsPlayed, myCardsPlayed.ToArray());
    }
}
