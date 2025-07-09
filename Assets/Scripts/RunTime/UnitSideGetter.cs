using UnityEngine;

public static class UnitSideGetter
{

    public static Side GetUnitSide<T>(this T obj,int myID) where T : MonoBehaviour,ISide
    {
        return myID == obj.ownerID ? Side.PlayerSide : Side.EnemySide;
    }
}
