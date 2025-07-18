using Game.Monsters;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class StatusConditionMethods
{
    public static void CheckParesis_Monster<T>(this T unit, Animator animator) where T : UnitBase
    {
        var paresis = unit.statusCondition.Paresis.isActive;
        var currentAnimatorSpeed = animator.speed;
        var isFreeze = unit.statusCondition.Freeze.isActive;
        var ChangeableSpeed = currentAnimatorSpeed != 0 && !isFreeze;
        var newSpeed = animator.speed / 2;
        if (paresis && ChangeableSpeed)//
        {
            Debug.Log("��გ��ł�");
            animator.speed = newSpeed;
        }
        else if (!paresis && ChangeableSpeed)
        {
            Debug.Log("��Ⴢ�����܂���");
            animator.speed = unit.originalAnimatorSpeed;
        }
    }

    public static void CheckFreeze_Unit<T>(this T unit, Animator animator) where T : UnitBase
    {
        var isFreezed = unit.statusCondition.Freeze.isActive;
        var isDead = unit.isDead;
        if (isFreezed && !isDead) animator.speed = 0f;
        else if (!isFreezed && !isDead)
        {
           if(unit is IMonster)
           {
                var type = unit.GetType();
                Debug.Log(type);
                var generic = typeof(MonsterControllerBase<>).MakeGenericType(type);
                if(generic.IsInstanceOfType(unit))
                {
                    var AttackStateProp = generic.GetProperty("AttackState");
                    var attackStateObj = AttackStateProp.GetValue(unit);
                    if (attackStateObj != null)
                    {
                        if(attackStateObj is IAttackState attackState)
                        {
                            Debug.Log("���B����");
                            var interval = attackState.isInterval;
                            if (!interval) animator.speed = unit.originalAnimatorSpeed;
                        }
                    }
                }
           }
           //���Ƃ���Player���ǉ�����ȉ�
        }
    }
}