using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using Game.Monsters;
using System.Reflection;
using Game.Monsters.KingDragon;

public static class AnimatorMethod
{
    public static AnimationClip GetAnimationClip(this Animator animator,string wantClipName)
    { 
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        var clips = controller.animationClips;
        foreach (var clip in clips)
        {
            Debug.Log(clip.name);
            if (clip.name == wantClipName) return clip;
        }

        Debug.LogError("指定されたアニメーションのクリップはこのアニメーターに存在しません");
        return null;
    }
    public static float GetRepeatInterval(this Animator animator,float startNormalizeTime, int repeatCount,
                                          float clipLength,float stateAnimSpeed)
    {
        var now = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        var elapsedNormalizedTime = now - startNormalizeTime;
        var startLength = elapsedNormalizedTime * clipLength;
        var leftLength = clipLength - startLength;
        var repeatInterval = leftLength / (float)repeatCount;
        return (repeatInterval / stateAnimSpeed) / animator.speed;
    }
    public static float GetCurrentNormalizedTime(this Animator animator,float startNormalizeTime)
    {
        var current = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        return current - startNormalizeTime;
    }

    public static void ChangeClipForAnimationEvent(this Animator animator,AnimatorOverrideController overrideController,
                                                   string clipName,float clipLength)
    {
        var attackMotionClip = animator.GetAnimationClip(clipName);
        var eventSetTime = clipLength - 0.01f;

        var newClip = UnityEngine.Object.Instantiate(attackMotionClip);
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.functionName = "StopAnimation_AttackState";
        newClip.name = clipName + "_PlusEvent";
        Debug.Log($"新しいくりっぷの名前{newClip.name},{eventSetTime}" );
        animationEvent.time = eventSetTime;
        newClip.AddEvent(animationEvent);

        overrideController[attackMotionClip.name] = newClip;
    }

    public static void StopAnimation(this UnitBase unit,float interval)
    {
        (object stateObj, PropertyInfo prop, FieldInfo field, CancellationTokenSource cts) infos;
        var animator = unit.GetComponent<Animator>();
        var stateType = unit is IMonster ? typeof(AttackStateBase<>).MakeGenericType(unit.GetType())
               : unit is KingDragonController ? typeof(AttackState) : default;
        Func<Type,(object,PropertyInfo,FieldInfo,CancellationTokenSource)> getAction = (stateType) =>
        {
            var isIntervalProp = stateType.GetProperty("isInterval", BindingFlags.Public | BindingFlags.Instance);
            var isAttackingField = stateType.GetField("isAttacking", BindingFlags.NonPublic 
                                                | BindingFlags.Public | BindingFlags.Instance);
            MemberInfo ctsMember = unit is IMonster ? stateType.GetField("cts", BindingFlags.NonPublic | BindingFlags.Instance)
                           : stateType.GetProperty("cts", BindingFlags.Public | BindingFlags.Instance);
            var targetUnitType = unit is IMonster ? typeof(MonsterControllerBase<>).MakeGenericType(unit.GetType())
                                 :typeof(KingDragonController);
            var stateProp = targetUnitType.GetProperty("AttackState");
            var attackStateObj = stateProp.GetValue(unit);
            var cts =  ctsMember is PropertyInfo propertyInfo? propertyInfo.GetValue(attackStateObj) as CancellationTokenSource
                       : ctsMember is FieldInfo fieldInfo ? fieldInfo.GetValue(attackStateObj) as CancellationTokenSource 
                       :default;
            return (attackStateObj, isIntervalProp, isAttackingField, cts);
        };
        infos = getAction(stateType);
        WaitInterval(animator,infos.stateObj, interval,infos.prop,infos.field,infos.cts);
    }
    static async void WaitInterval(Animator animator,object stateObj,float interval
                                   ,PropertyInfo isIntervalProp,FieldInfo isAttackingField,CancellationTokenSource cts)
    {
       
        isIntervalProp.SetValue(stateObj, true);
        animator.speed = 0f;
        try
        {
            var stopAnimInterval = animator.speed != 0f ? interval / animator.speed : interval;
            await UniTask.Delay(TimeSpan.FromSeconds(stopAnimInterval), cancellationToken: cts.Token);
        }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
        finally
        {
            isIntervalProp.SetValue(stateObj, false);
            isAttackingField.SetValue(stateObj, false);
            Debug.Log($"止まり終えました,{interval},{isIntervalProp.GetValue(stateObj)},{isAttackingField.GetValue(stateObj)}");
        }
    }
}
