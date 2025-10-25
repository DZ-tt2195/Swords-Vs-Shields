using UnityEngine;
using Photon.Pun;

[CreateAssetMenu(fileName = "ExchangeOne", menuName = "ScriptableObjects/ExchangeOne")]
public class ExchangeOne : ExchangeInfo
{
    public override void ForPlayer(Player player)
    {
        Debug.Log(player.OtherPlayerInteraction(thisRound));
        Debug.Log(player.OtherPlayerInteraction(-1*thisRound));
    }
}
