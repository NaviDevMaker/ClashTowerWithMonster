using UnityEngine;

public class StatusEffect
{
   public bool isActive = false;
   public float inverval = 0f;
   public int isEffectedCount = 0;
 
}
public enum StatusConditionType
{
    Freeze,
    Paresis,
    BuffSpeed,
    BuffPower,
    DemonCurse,
    Confusion,
}
