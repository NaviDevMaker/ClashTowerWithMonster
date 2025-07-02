using Game.Monsters;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class DebuffUnitMethods
{
   public static void CheckParesis_Monster<T>(this T unit,Animator animator) where T : UnitBase
    {
        var paresis = unit.statusCondition.Paresis.isActive;
        var currentAnimatorSpeed = animator.speed;
        var NotInteval = currentAnimatorSpeed != 0;
        if (paresis && NotInteval)
        {
            Debug.Log("–ƒáƒ’†‚Å‚·");
            animator.speed = 0.5f;
        }
        else if (!paresis && NotInteval)
        {
            Debug.Log("–ƒáƒ‚ªŽ¡‚è‚Ü‚µ‚½");
            animator.speed = 1.0f;
        }
    }
} 
