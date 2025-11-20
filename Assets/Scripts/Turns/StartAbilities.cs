using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StartAbilities", menuName = "ScriptableObjects/StartAbilities")]
public class StartAbilities : Turn
{
    public override void MasterStart()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        Log.inst.MasterText($"Use Green-Num-{currentRound}");
    }

    public override void ForPlayer(Player player)
    {
        player.endPause = false;
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
    }
}
