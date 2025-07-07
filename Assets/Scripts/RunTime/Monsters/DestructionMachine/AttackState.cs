using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Monsters.DestructionMachine
{
    public class AttackState : AttackStateBase<DestructionMachineController>
    {
        public AttackState(DestructionMachineController controller) : base(controller) { }
        LongDistanceAttack<DestructionMachineController> nextMover = null;
        readonly int reload = Animator.StringToHash("isReloading");
        bool isShotFirst = false;
        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if(attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<DestructionMachineController>(controller, this, clipLength,8);      
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
           if(nextMover != null) nextMover.gameObject.SetActive(false);
            isShotFirst = false;
            base.OnExit();
        }

        protected override async UniTask Attack_Long()
        {
            float startNormalizeTime = 0f;
            float now = 0f;
            try
            {
                controller.animator.speed = 1.0f;
                if (isShotFirst) await Reload();
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName)
                ,cancellationToken: cts.Token);
                Debug.Log(target.gameObject.name);
                startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - startNormalizeTime >= attackEndNomTime;
                });

                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);
                isShotFirst = true;
                SetNextCannonBallAndShot();
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            finally
            {
                if (cts.IsCancellationRequested)
                {
                    if (nextMover != null) nextMover.target = null;
                    leftLengthTime = Mathf.Max(0f, (now - startNormalizeTime) * clipLength) / stateAnimSpeed;
                    isAttacking = false;
                }
                else leftLengthTime = 0f;
            }
        }
        async UniTask Reload()
        {
            try
            {
                Debug.Log("Reload‚³‚ê‚Ü‚·");
                controller.animator.SetBool(reload, true);
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName("Reload"), cancellationToken: cts.Token);
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                controller.animator.SetBool(reload, false);
            }
        }
        void SetNextCannonBallAndShot()
        {

            if (nextMover != null) return;
            foreach (var mover in controller.movers)
            {
                if (!mover.gameObject.activeSelf)
                {
                    nextMover = mover;
                    break;
                }
            }
            if (nextMover != null)
            {
                var effectPos = nextMover.transform.position;
                var scaleAmount = 0.5f;
                EffectManager.Instance.expsionEffect.GenerateExplosionEffect(effectPos,scaleAmount);
                nextMover.target = this.target;
                nextMover.gameObject.SetActive(true);
                nextMover.Move();
                Debug.Log("‘Å‚½‚ê‚Ü‚µ‚½");
            }
        }

        public override void StopAnimFromEvent()
        {
            nextMover = null;
            base.StopAnimFromEvent();
        }
    }

}