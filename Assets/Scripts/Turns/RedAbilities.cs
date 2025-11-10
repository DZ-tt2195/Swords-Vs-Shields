using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RedAbilities", menuName = "ScriptableObjects/RedAbilities")]
public class RedAbilities : Turn
{
    public override void ForMaster()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        Log.inst.MasterText($"Use Red-Num-{currentRound}");
        Log.inst.MasterText("Blank");
    }

    public override void ForPlayer(Player player)
    {
        NextAbility(player, new());
    }

    void NextAbility(Player player, HashSet<Card> alreadyDone)
    {
        List<Card> myTroops = player.GetTroops();
        HashSet<Card> canDo = new();

        foreach (Card card in myTroops)
        {
            if (alreadyDone.Contains(card) || !card.CanUseAbility())
                continue;
            if (card.thisCard.CanUseAbiltyOne(player, card) == AbilityType.Attack)
                canDo.Add(card);
            if (card.thisCard.CanUseAbiltyTwo(player, card) == AbilityType.Attack)
                canDo.Add(card);
        }

        if (canDo.Count >= 1)
        {
            player.ChooseButtonInPopup(new() { new("Decline") }, "Use Red Instruction", new(0, 325), false);
            List<MiniCardDisplay> toChoose = new();
            foreach (MiniCardDisplay display in player.AliveTroops())
            {
                if (canDo.Contains(display.card))
                    toChoose.Add(display);
            }
            player.ChooseDisplayOnScreen(toChoose, ChooseToUse, false);

            void ChooseToUse(Card card)
            {
                Log.inst.AddMyText($"Resolve Card-Player-{player.name}-Card-{card.name}", true);
                if (card.thisCard.CanUseAbiltyOne(player, card) == AbilityType.Attack)
                    Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityOne(player, card, 1), 1);
                if (card.thisCard.CanUseAbiltyTwo(player, card) == AbilityType.Attack)
                    Log.inst.NewDecisionContainer(() => card.thisCard.DoAbilityTwo(player, card, 1), 1);

                HashSet<Card> newSet = new(alreadyDone);
                newSet.Add(card);
                Log.inst.NewDecisionContainer(() => NextAbility(player, newSet), 0);
            }
        }
    }
}
