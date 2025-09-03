using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.DestructionMachine
{
    public class AttackState : AttackStateBase<DestructionMachineController>, AttackStateBase<DestructionMachineController>.ILongDistanceAction
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

        protected override async UniTask Attack_Long(Func<LongDistanceAttack<DestructionMachineController>> getNextMover = null,
            UnityAction<LongDistanceAttack<DestructionMachineController>> moveAction = null)
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
            catch (OperationCanceledException)
            {
                if (nextMover != null) nextMover.target = null;
                var elaspedTime = (now - startNormalizeTime) * clipLength;
                leftLengthTime = Mathf.Max(0f,clipLength - elaspedTime) / stateAnimSpeed;
                isAttacking = false;
            }
            catch (ObjectDisposedException) { }
            finally{ }
            leftLengthTime = 0f;
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
            nextMover = GetNextMover();
            NextMoverAction(nextMover);
        }

        public override void StopAnimFromEvent()
        {
            nextMover = null;
            base.StopAnimFromEvent();
        }

        public LongDistanceAttack<DestructionMachineController> GetNextMover()
        {
            foreach (var mover in controller.movers)
            {
                if (!mover.gameObject.activeSelf)
                {
                    return mover;
                }
            }
            return null;
        }

        public void NextMoverAction(LongDistanceAttack<DestructionMachineController> nextMover)
        {
            if (nextMover != null)
            {
                var effectPos = nextMover.transform.position;
                var scaleAmount = 0.5f;
                EffectManager.Instance.expsionEffect.GenerateExplosionEffect(effectPos, scaleAmount);
                nextMover.target = this.target;
                nextMover.gameObject.SetActive(true);
                nextMover.Move();
                Debug.Log("‘Å‚½‚ê‚Ü‚µ‚½");
            }
        }
    }

}