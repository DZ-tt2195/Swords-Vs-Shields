using UnityEngine;

public class Turn : ScriptableObject
{
    public virtual void ForMaster()
    {

    }

    public virtual void ForPlayer(Player player)
    {
        //Log.inst.NewDecisionContainer(this, () => InstantDraw(player), 0);
    }
}
