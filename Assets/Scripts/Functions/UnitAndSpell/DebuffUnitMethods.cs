using Game.Monsters;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class DebuffUnitMethods
{
   public static void CheckParesis_Monster<T>(this T unit,Animator animator) where T : UnitBase
    {
        var paresis = unit.statusCondition.Paresis.isActive;
        var currentAnimatorSpeed = animator.speed;
        var notZeroSpeed = currentAnimatorSpeed != 0;
        if (paresis && notZeroSpeed)
        {
            Debug.Log("��გ��ł�");
            animator.speed = 0.5f;
        }
        else if (!paresis && notZeroSpeed)
        {
            Debug.Log("��Ⴢ�����܂���");
            animator.speed = 1.0f;
        }
    }
} 
