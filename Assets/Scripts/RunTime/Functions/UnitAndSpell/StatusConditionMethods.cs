using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

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
            Debug.Log("–ƒáƒ’†‚Å‚·");
            animator.speed = newSpeed;
        }
        else if (!paresis && ChangeableSpeed)
        {
            Debug.Log("–ƒáƒ‚ªŽ¡‚è‚Ü‚µ‚½");
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
                            Debug.Log("“ž’BŠ®—¹");
                            var interval = attackState.isInterval;
                            if (!interval) animator.speed = unit.originalAnimatorSpeed;
                        }
                    }
                }
           }
           //‚ ‚Æ‚©‚çPlayer‘¤’Ç‰Á‚µ‚ë‚È‰´
        }
    }

    public static async void ParesisTarget<T>(this T controller,UnitBase target) where T : UnitBase 
    {  
        if (target == null || target is TowerController) return;
        var statusCondition = target.statusCondition;
        try
        {
            if (statusCondition != null)
            {
                Debug.Log("–ƒáƒ‚³‚¹‚Ü‚·");
                var interval = controller.statusCondition.Paresis.inverval;
                statusCondition.Paresis.isActive = true;
                EffectManager.Instance.statusConditionEffect.paresisEffect.GenerateParesisEffect(target,interval);
                statusCondition.Paresis.isEffectedCount++;
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: target.GetCancellationTokenOnDestroy());
                statusCondition.Paresis.isEffectedCount--;
                if (statusCondition.Paresis.isEffectedCount == 0) statusCondition.Paresis.isActive = false;
            }
        }
        catch (OperationCanceledException) { }     
    }
}