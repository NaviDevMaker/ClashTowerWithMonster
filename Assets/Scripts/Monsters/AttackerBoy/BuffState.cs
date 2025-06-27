using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Game.Monsters.AttackerBoy
{
    public class BuffState : BuffStateBase<AttackerBoyController>
    {
       public BuffState(AttackerBoyController controller): base(controller) { }
        public override void OnEnter()
        {
            if (radius == 0f) radius = 8f;
            buffType = BuffType.Power;
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        //protected override async UniTask<List<UnitBase>> GetUnitInRange()
        //{
        //    var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject, 8f);
        //    if (sortedArray.Length == 0) return new List<UnitBase>();
        //    var filteredList = sortedArray.Where(unit =>
        //    {
        //        var isDead = unit.isDead;
        //        var side = unit.Side;
        //        if (isDead || side == Side.EnemySide) return false;             
        //        var isBuffed = unit.statusCondition.BuffPower.isActive;
        //        var isUnit = unit is IMonster || unit is IPlayer;
        //        if (isBuffed || !isUnit) return false;
        //        return true;
        //    }).ToList();
        //    if (filteredList.Count > buffUnitCount)
        //    {
        //        while (filteredList.Count == 3)
        //        {
        //            var last = filteredList.Count - 1;
        //            filteredList.RemoveAt(last);
        //            await UniTask.Yield();
        //        }
        //        return filteredList;
        //    }
        //    else return filteredList;
        //}
        //protected override async void Buff()
        //{
        //    foreach (var unit in unitInBuffRange)
        //    {
        //        unit.statusCondition.BuffPower.isActive = true;
        //        EffectManager.Instance.statusConditionEffect.buffEffect.SetEffectToUnit(unit, BuffType.Power);
        //    }
        //    unitInBuffRange.ForEach(unit => Debug.Log($"{unit.name}‚ÌUŒ‚—Í‚ªƒoƒt‚³‚ê‚Ü‚µ‚½"));
        //    var clipName = AnimatorClipGeter.GetAnimationClip(controller.animator, "Buff").name;
        //    await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName(clipName)
        //    ,cancellationToken:controller.gameObject.GetCancellationTokenOnDestroy());
        //    await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
        //    ,cancellationToken:controller.gameObject.GetCancellationTokenOnDestroy());
        //    buffMoveEnd = true;
        //    ResetTime?.Invoke();
        //}
    }
}

