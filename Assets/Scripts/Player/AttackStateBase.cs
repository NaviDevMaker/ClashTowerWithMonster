using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
namespace Game.Players
{
    public class AttackStateBase<T> : StateMachineBase<T>,IUnitAttack where T : PlayerControllerBase<T>
    {
        public UnitBase target;
        public AttackStateBase(T controller) : base(controller) { }
        CancellationTokenSource cts = new CancellationTokenSource();

        bool isAttacking = false;
        protected float interval = 0f;
        public override void OnEnter()
        {
            cts = new CancellationTokenSource();
            if (clipLength == 0)
            {
                clipLength = controller.GetAnimClipLength();
            }
        }
        public override void OnUpdate()
        {
            if(target != null)
            {
                LookEnemyDirection();
            }
            Debug.Log(target);
            if(!isAttacking)
            {
                isAttacking = true;
                controller.OnAttackingPlayer?.Invoke(isAttacking);
                Attack();
            }
            if(target == null)
            {
                nextState = controller.IdleState;
                controller.ChangeState(nextState);
                Debug.Log("����������");
            }
            else if (InputManager.IsClickedMoveWhenAttacking())//
            {
                nextState = controller.MoveState;
                controller.ChangeState(nextState);
            }

        }
        public override void OnExit()
        {
            cts.Cancel();
            cts.Dispose();
            isAttacking = false;
            controller.OnAttackingPlayer?.Invoke(isAttacking);
        }
        public void Attack()
        {
            controller.animator.SetTrigger(controller.AnimatorPar.Attack);
        }

        void LookEnemyDirection()
        {
            var direction = target.transform.position - controller.transform.position;
            var rotation = Quaternion.LookRotation(direction);
           controller.transform.rotation = rotation;

        }
        public void Attack_SimpleFromAnimEvent()
        {
            if (target != null && target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                Debug.Log($"{controller.gameObject.name}�̃A�^�b�N");
                unitDamagable.Damage(controller.PlayerStatus.AttackAmount);
                EffectManager.Instance.hitEffect.GenerateHitEffect(target);
            }
            //controller.animator.speed = 0f;         
        }

        public async void WaitIntervalFromAnimEvent()
        {
            if (cts.IsCancellationRequested) return;
            try
            {
                //�@
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: cts.Token);
            }
            catch(ObjectDisposedException)
            {
                //�񓯊��̃L�������Ň@�̎���OnExit()�����荞��ł������p
                return;
            }
            catch (OperationCanceledException)
            {
                // ����ɃL�����Z�����ꂽ �� ��������OK
                return;
            }
            isAttacking = false;
            controller.OnAttackingPlayer?.Invoke(isAttacking);
        }
    }
}


