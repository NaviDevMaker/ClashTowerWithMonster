using Game.Monsters;
using Unity.VisualScripting;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System;
public class BuffStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
{
    readonly int buff = Animator.StringToHash("isBuffing");
    protected int buffUnitCount = 3;
    protected float radius = 0f;
    protected  List<UnitBase> unitInBuffRange = new List<UnitBase>();
    protected bool buffMoveEnd = false;
    public bool wasBuffedFailed = false;
    public bool unitIsRange = false;
    public UnityAction ResetTime;
    Action OnBuffMoveEnd;
    protected BuffType buffType;
    public BuffStateBase(T controller) : base(controller) { }
    public override async void OnEnter()
    {
        OnBuffMoveEnd = (() => buffMoveEnd = true);
        try
        {
            controller.ChaseState.cts?.Cancel();
        }
        catch(ObjectDisposedException)
        {
            Debug.Log("Šù‚ÉDispose‚³‚ê‚Ü‚µ‚½");
        }
        nextState = controller.ChaseState;
        unitInBuffRange = await controller.GetUnitInRange<T>(radius,buffUnitCount,buffType);
        Debug.Log(unitInBuffRange.Count);
        if(unitInBuffRange.Count == 0)
        {
            buffMoveEnd = true;
            wasBuffedFailed = true;
            unitIsRange = false;
            return;
        }

        wasBuffedFailed = false;
        unitIsRange = true;
        controller.animator.SetTrigger(buff);
        controller.Buff(unitInBuffRange,controller.animator,OnBuffMoveEnd,ResetTime,buffType);
    }
    public override void OnExit()
    {
        buffMoveEnd = false;
    }
    public override void OnUpdate()
    {
        if(buffMoveEnd)
        {
            controller.ChangeState(nextState);
        }
    }
    //protected virtual async void Buff()
    //{
    //    await UniTask.CompletedTask;
    //}
    public async void CheckIsUnitInRange()
    {
        if (!wasBuffedFailed) unitIsRange = false;
        else
        {
            var serchedTargets = await controller.GetUnitInRange<T>(radius,buffUnitCount,buffType);
            if (serchedTargets.Count > 0) unitIsRange = true;
        }
    }
    //protected virtual async UniTask<List<UnitBase>> GetUnitInRange()
    //{
    //    await UniTask.CompletedTask;
    //    return new List<UnitBase>();
    //}
}
