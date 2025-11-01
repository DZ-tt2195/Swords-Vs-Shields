using UnityEngine;

[CreateAssetMenu(fileName = "PlayCard", menuName = "ScriptableObjects/PlayCard")]
public class PlayCard : Turn
{
    public override void ForMaster()
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        Log.inst.MasterText(Translator.inst.Translate("Blank"));
        Log.inst.MasterText(Translator.inst.Translate("Play Card", new() { ("Num", currentRound.ToString())}));
    }

    public override void ForPlayer(Player player)
    {
        int currentRound = (int)PhotonCompatible.GetRoomProperty(RoomProp.CurrentRound);
        player.DrawCardRPC(currentRound == 1 ? 4 : 2, 0);
        player.ActionRPC(2, 0);
        player.GreenCoinRPC(currentRound - player.GetInt(PlayerProp.GreenCoin.ToString()), 0);
        player.RedCoinRPC(currentRound - player.GetInt(PlayerProp.RedCoin.ToString()), 0);
        Log.inst.NewDecisionContainer(() => PlayLoop(player), 0);
    }

    void PlayLoop(Player player)
    {

    }
}
