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
        player.ActionRPC(2, 0);
        player.GreenCoinRPC(currentRound - TurnManager.inst.GetInt(PlayerProp.GreenCoin.ToString()), 0);
        player.RedCoinRPC(currentRound - TurnManager.inst.GetInt(PlayerProp.RedCoin.ToString()), 0);
        Log.inst.NewDecisionContainer(() => PlayLoop(player), 0);
    }

    void PlayLoop(Player player)
    {

    }
}
