using UnityEngine;

[CreateAssetMenu(fileName = "Wait", menuName = "ScriptableObjects/Wait")]
public class Wait : Turn
{
    public override void ForPlayer(Player player)
    {
        player.endPause = false;
    }
}
