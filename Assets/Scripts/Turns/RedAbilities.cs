using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RedAbilities", menuName = "ScriptableObjects/RedAbilities")]
public class RedAbilities : Turn
{
    public override void MasterStart()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(ConstantStrings.CurrentRound);
        Log.inst.MasterText($"Use Red-Num-{currentRound}");
    }

    public override void MasterEnd()
    {
        Log.inst.MasterText($"Blank");
    }

    public override void ForPlayer(Player player)
    {
        NextAbility(player, new());
    }

    void NextAbility(Player player, HashSet<Card> alreadyDone)
    {
        List<MiniCardDisplay> redCards = new();

        foreach (MiniCardDisplay display in player.AliveTroops())
        {
            Card card = display.card;
            if (alreadyDone.Contains(card) || !card.CanUseAbility())
                continue;
            if (card.thisCard.HasType(AbilityType.Attack, player, card, -1))
                redCards.Add(display);
        }

        if (redCards.Count >= 1)
        {
            MakeDecision.inst.ChooseDisplayOnScreen(redCards, "Use Red Instruction", ChooseToUse, false);

            void ChooseToUse(Card card)
            {
                Log.inst.AddMyText($"Resolve Card-Player-{player.name}-Card-{card.name}", false);
                card.thisCard.HasType(AbilityType.Attack, player, card, 1);

                HashSet<Card> newSet = new(alreadyDone);
                newSet.Add(card);
                Log.inst.NewDecisionContainer(() => NextAbility(player, newSet), 0);
            }
        }
    }
}
