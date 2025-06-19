using UnityEngine;

public class StatusEffect
{
   public bool isActive = false;
   public float inverval = 0f;
   public int isAttackedCount = 0;

   
}

public interface IStatusCondition
{
   StatusEffect Paresis { get;}
}
