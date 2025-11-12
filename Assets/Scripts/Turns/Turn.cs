using UnityEngine;

public class Turn : ScriptableObject
{
    public virtual void MasterStart()
    {

    }

    public virtual void ForPlayer(Player player)
    {
        //Log.inst.NewDecisionContainer(this, () => InstantDraw(player), 0);
    }

    public virtual void MasterEnd()
    {

    }

}
