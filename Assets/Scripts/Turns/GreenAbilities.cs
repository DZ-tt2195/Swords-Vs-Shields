using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GreenAbilities", menuName = "ScriptableObjects/GreenAbilities")]
public class GreenAbilities : Turn
{
    public override void MasterEnd()
    {
        Log.inst.MasterText(true, AutoTranslate.Blank());
    }

    public override void ForPlayer(Player player)
    {
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
            MakeDecision.inst.ChooseDisplayOnScreen(greenCards, AutoTranslate.Use_Green_Instruction(), ChooseToUse, false);

            void ChooseToUse(Card card)
            {
                Log.inst.AddMyText(false, OnlineTranslate.Online_Resolve_Card(player.name, card.name));
                card.thisCard.HasType(AbilityType.Defend, player, card, 1);

                HashSet<Card> newSet = new(alreadyDone);
                newSet.Add(card);
                Log.inst.NewDecisionContainer(() => NextAbility(player, newSet), 0);
            }
        }

        if (player.GetAction() >= 1)
        {
            if (greenCards.Count == 0)
                MakeDecision.inst.ChooseTextButton(new() { new(AutoTranslate.Done(), Decline) }, AutoTranslate.Use_Green_Instruction(), false);

            List<Card> myHand = player.GetHand();
            MakeDecision.inst.ChooseCardOnScreen(myHand, AutoTranslate.Use_Green_Instruction(), ChooseToPlay, false);

            void ChooseToPlay(Card card)
            {
                Log.inst.AddMyText(true, OnlineTranslate.Online_Play_Card(player.name, card.name));
                player.ActionRPC(-1, -1);

                card.HealthRPC(player, card.thisCard.dataFile.startingHealth, -1);
                Log.inst.NewRollback(() => HandToPlay(player, card));
                card.thisCard.HasType(AbilityType.Play, player, card, 1);
                Log.inst.NewDecisionContainer(() => NextAbility(player, alreadyDone), 0);
            }

            void Decline()
            {
                Log.inst.AddMyText(false, OnlineTranslate.Online_End_Turn(player.name));
            }
        }
    }

    void HandToPlay(Player player, Card cardToPlay)
    {
        List<Card> myHand = player.GetHand();
        List<Card> myTroops = player.GetTroops();
        List<string> myCardsPlayed = TurnManager.inst.GetStringList(ConstantStrings.AllCardsPlayed, player);

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
            int currentRound = (int)PhotonCompatible.GetRoomProperty(ConstantStrings.CurrentRound);
            myCardsPlayed.Add(OnlineTranslate.Online_Played_Card_Info(cardToPlay.name, currentRound.ToString()));
        }
        TurnManager.inst.WillChangePlayerProperty(player, ConstantStrings.MyHand, TurnManager.inst.ConvertCardList(myHand)); player.uiDictionary[ConstantStrings.MyHand] = true;
        TurnManager.inst.WillChangePlayerProperty(player, ConstantStrings.MyTroops, TurnManager.inst.ConvertCardList(myTroops)); player.uiDictionary[ConstantStrings.MyTroops] = true;
        TurnManager.inst.WillChangePlayerProperty(player, ConstantStrings.AllCardsPlayed, myCardsPlayed.ToArray());
    }
}
