using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StartAbilities", menuName = "ScriptableObjects/StartAbilities")]
public class StartAbilities : Turn
{
    public override void MasterStart()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(ConstantStrings.CurrentRound);
        Log.inst.MasterText(true, "Use_Green", "", "", currentRound.ToString());
    }

    public override void ForPlayer(Player player)
    {
        player.endPause = false;
        int currentRound = (int)PhotonCompatible.GetRoomProperty(ConstantStrings.CurrentRound);
        player.DrawCardRPC(currentRound == 1 ? 4 : 2);

        player.ActionRPC(2);
        int nextRoundAction = TurnManager.inst.GetInt(ConstantStrings.NextRoundAction, player);
        player.ActionRPC(nextRoundAction, 1);
        TurnManager.inst.WillChangePlayerProperty(player, ConstantStrings.NextRoundAction, 0);

        player.ShieldRPC(currentRound - player.GetShield());
        int nextRoundShield = TurnManager.inst.GetInt(ConstantStrings.NextRoundShield, player);
        player.ShieldRPC(nextRoundShield, 1);
        TurnManager.inst.WillChangePlayerProperty(player, ConstantStrings.NextRoundShield, 0);

        player.SwordRPC(currentRound - player.GetSword());
        int nextRoundSword = TurnManager.inst.GetInt(ConstantStrings.NextRoundSword, player);
        player.SwordRPC(nextRoundSword, 1);
        TurnManager.inst.WillChangePlayerProperty(player, ConstantStrings.NextRoundSword, 0);
    }
}
