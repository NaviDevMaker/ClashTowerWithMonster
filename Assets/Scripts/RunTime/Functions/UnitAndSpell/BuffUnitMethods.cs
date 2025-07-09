using Cysharp.Threading.Tasks;
using Game.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;

public static class BuffUnitMethods
{
    public static async UniTask<List<UnitBase>> GetUnitInRange<T>(this UnitBase originUnit,float radius
        ,int buffUnitCount,BuffType buffType) where T : MonoBehaviour
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(originUnit.gameObject, radius);
        if (sortedArray.Length == 0) return new List<UnitBase>();
        var filteredList = sortedArray.Where(unit =>
        {
            if(unit.TryGetComponent<ISummonbable>(out var summonbable))
            {
                var isSummoned = summonbable.isSummoned;
                if (!isSummoned) return false;
            }
            var isDead = unit.isDead;
            var unitSide = unit.GetUnitSide(originUnit.ownerID);
            var effectiveSide = Side.PlayerSide;
            if (isDead || (unitSide & effectiveSide) == 0) return false;
            if (unit.statusCondition == null) return false;
            var isBuffed = false;
            isBuffed = buffType switch
            {
                BuffType.Power => unit.statusCondition.BuffPower.isActive,
                BuffType.Speed => unit.statusCondition.BuffSpeed.isActive,
                _ => default
            };
            var isUnit = unit is IMonster || unit is IPlayer;
            if (isBuffed || !isUnit) return false;
            return true;
        }).ToList();
        if (filteredList.Count > buffUnitCount)
        {
            while (filteredList.Count > buffUnitCount)
            {
                var last = filteredList.Count - 1;
                filteredList.RemoveAt(last);
                await UniTask.Yield();
            }
            return filteredList;
        }
        else return filteredList;
    }

    public static async void Buff<T>(this T controller,List<UnitBase> unitInBuffRange
        ,Animator animator,Action onEndBuffMove,UnityAction resetTime,BuffType buffType) where T : UnitBase
    {
        List<UniTask> tasks = new List<UniTask>();

        try
        {
            foreach (var unit in unitInBuffRange)
            {
                if (unit == null) return;
                var task = EffectManager.Instance.statusConditionEffect.buffEffect.SetEffectToUnit(controller, unit, buffType);
                tasks.Add(task);
            }
            await UniTask.WhenAll(tasks);
            foreach (var unit in unitInBuffRange)
            {
                if (unit == null) return;
                if (buffType == BuffType.Speed) unit.statusCondition.BuffSpeed.isActive = true;
                else if (buffType == BuffType.Power) unit.statusCondition.BuffPower.isActive = true;
            }
            unitInBuffRange.ForEach(unit => Debug.Log($"{unit.name}のスピードがバフされました"));
            if (animator == null) return;
            var clip = AnimatorClipGeter.GetAnimationClip(animator, "Buff");
            if (clip == null) return;

            var clipName = clip.name;
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(clipName)
            , cancellationToken: controller.gameObject.GetCancellationTokenOnDestroy());
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            , cancellationToken: controller.gameObject.GetCancellationTokenOnDestroy());
        }          
        catch (OperationCanceledException) { }
        catch (Exception) { }
        onEndBuffMove?.Invoke();
        resetTime?.Invoke();
    }

    public static int BuffStatus<T>(this T unit,BuffType buffType,int amount) where T : UnitBase
    {     
        var isBuffed = buffType switch
        {
            BuffType.Power => unit.statusCondition.BuffPower.isActive,
            BuffType.Speed => unit.statusCondition.BuffSpeed.isActive,
            _ => false
        };

        if (!isBuffed) return amount;
        var increaseRatio = 1.3f;
        var newAmount = Mathf.RoundToInt(amount * increaseRatio);
        return newAmount;      
    }


}
