using UnityEngine;

[CreateAssetMenu(fileName = "Ending", menuName = "ScriptableObjects/Ending")]
public class Ending : Turn
{
    public override void ForPlayer(Player player)
    {
        player.endPause = false;
    }
}
