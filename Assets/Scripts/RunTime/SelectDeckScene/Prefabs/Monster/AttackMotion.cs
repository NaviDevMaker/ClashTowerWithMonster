using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AttackMotion
{
    public void AnimationEventSetup(ISelectableMonster selectableMonster)
    {
        var animator = selectableMonster.animator;
        var runTimeAnimator = animator.runtimeAnimatorController;
        var clip = runTimeAnimator.animationClips.ToList().FirstOrDefault(clip => clip.name == "Attack");
        var clipLength = clip.length;
        var eventSetTime = clipLength - 0.01f;

        var originalController = animator.runtimeAnimatorController;
        var overrideController = new AnimatorOverrideController(originalController);
        animator.runtimeAnimatorController = overrideController;

        var newClip = UnityEngine.Object.Instantiate(clip);
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.functionName = "AppearAttackMotion";
        newClip.name = clip.name;
        animationEvent.time = eventSetTime;
        newClip.AddEvent(animationEvent);

        overrideController[clip.name] = newClip;
    }
    public async void SimpleAttackMotion(SelectableMonster selectableMonster,CancellationTokenSource motionCls)
    {
        var animator = selectableMonster.animator;
        try
        {

            //Emission‚ªI‚í‚é‚Ü‚Å‚ðˆÓ–¡‚·‚é
            await UniTask.WaitUntil(() => animator.speed == 1.0f, cancellationToken: motionCls.Token);
        }
        catch (OperationCanceledException) { return; }
        var animPar = selectableMonster.monsterAnimatorPar;
        selectableMonster.currentMotionCls = motionCls;
        animator.SetBool(animPar.Attack_Hash, true);
        animator.Play("Attack");
        try
        {
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"), cancellationToken: motionCls.Token);
        }
        catch (OperationCanceledException) { return; }
    }
    public async void DestractionMachineAttack(SelectableMonster selectableMonster, CancellationTokenSource motionCls)
    {
        var animator = selectableMonster.animator;
        try
        {
            //Emission‚ªI‚í‚é‚Ü‚Å‚ðˆÓ–¡‚·‚é
            await UniTask.WaitUntil(() => animator.speed == 1.0f, cancellationToken: motionCls.Token);
        }
        catch (OperationCanceledException) { return; }
        var animPar = selectableMonster.monsterAnimatorPar;
        animator.SetBool(animPar.Attack_Hash, true);
        animator.Play("Attack");
        Func<string,UniTask> waitMotionEnd = async (motionName) =>
        {
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(motionName),cancellationToken:motionCls.Token);
            await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f,cancellationToken: motionCls.Token);
        };

        var attack = "Attack";
        var reload = "Reload";
        try
        {
            while (!motionCls.IsCancellationRequested)
            {
                await waitMotionEnd(attack);
                animator.SetBool("isReloading", true);
                await waitMotionEnd(reload);
                animator.SetBool("isReloading", false);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            animator.SetBool("isReloading", false);
            animator.SetBool(selectableMonster.monsterAnimatorPar.Attack_Hash, false);
            animator.Play("Idle");
        }
    }
}
