using Cysharp.Threading.Tasks;
using Game.Monsters.FylingDemon;

using System;
using System.Threading;
using UnityEngine;

namespace Game.Monsters.FylingDemon
{
    public class CallHelpState : StateMachineBase<FylingDemonController>
    {
        readonly int callHelp = Animator.StringToHash("isCallingHelp");
        public CallHelpState(FylingDemonController controller) : base(controller) { }

        CancellationTokenSource callHelpCts = new CancellationTokenSource();
        bool IsEndCallHelp = false;
        public override void OnEnter()
        {
            callHelpCts = new CancellationTokenSource();
            try
            {
                controller.ChaseState.cts?.Cancel();
            }
            catch (ObjectDisposedException) { }
            nextState = controller.ChaseState;
            CallHelp();
        }
        public override void OnUpdate()
        {
            if (IsEndCallHelp) controller.ChangeState(nextState);
        }
        public override void OnExit()
        {
            IsEndCallHelp = false;
            controller.elapsedTime = 0f;
            //‚±‚êCallHelp’†‚ÉŽ€‚ñ‚¾‚Æ‚«—p
            callHelpCts?.Cancel();
            callHelpCts?.Dispose();
        }
        async void CallHelp()
        {
            try
            {
                var doubleCts = CancellationTokenSource.CreateLinkedTokenSource(callHelpCts.Token, controller.GetCancellationTokenOnDestroy());          

                if(controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.Attack))
                {
                    Func<bool> waitAttackEnd = () =>
                    {
                        if (controller.isDead) return false;
                        return controller.AttackState.isInterval
                        || !(controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.Attack));
                    };
                    await UniTask.WaitUntil(waitAttackEnd, cancellationToken: doubleCts.Token);
                }

                controller.animator.SetTrigger(callHelp);
                Func<bool> wait = () =>
                {
                    return controller.animator.GetCurrentAnimatorStateInfo(0).IsName("CallHelp");
                };
                await UniTask.WaitUntil(wait, cancellationToken: doubleCts.Token);
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f,cancellationToken: doubleCts.Token);
                var rot = controller.transform.rotation;
                var zOffset = controller.BodyMesh.bounds.size.z * 1.05f;
                var pos = controller.transform.position - (controller.transform.forward * zOffset);
                var flyingDemon = UnityEngine.Object.Instantiate(controller.demonObj, pos, rot);
                var demonCmp = flyingDemon.GetComponent<FylingDemonController>();
                var flatPos = PositionGetter.GetFlatPos(pos);
                controller.SetSummonParticle(flatPos + Vector3.up * 0.5f);
                demonCmp.ownerID = controller.ownerID;
                demonCmp.isSummoned = true;
                IsEndCallHelp = true;
            }
            catch (OperationCanceledException) { }
            finally { }
        }
    }
}

