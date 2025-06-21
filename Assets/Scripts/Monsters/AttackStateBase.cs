using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEditor.Build.Pipeline;

namespace Game.Monsters
{
    public class AttackStateBase<T> : StateMachineBase<T>, IUnitAttack where T : MonsterControllerBase<T>
    {
        public AttackStateBase(T controller) : base(controller) { }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        //Vector3 towerTargetPos;
        public UnitBase target;
        public float flyingOffsetY = 0f;
        int attackAmount = 0;
        float attackRange = 0f;
        protected CancellationTokenSource cts;

        public int maxFrame = 0;
        public int attackEndFrame = 0;
        public float attackEndNomTime = 0f;
        public float interval = 0f;
        protected bool isAttacking = false;
        bool isSettedEventClip = false;
        public override void OnEnter()
        {
            attackAmount = controller.BuffStatus(BuffType.Power, controller.MonsterStatus.AttackAmount);
            Debug.Log("Attackに入りました");
            if (attackRange == 0f)
            {
                attackRange = GetAttackRange();
            }
                cts = new CancellationTokenSource();
            controller.animator.SetBool(controller.MonsterAnimPar.Attack, true);
            if (clipLength == 0)
            {
                clipLength = controller.GetAnimClipLength();
            }
            if(!isSettedEventClip) ChangeClipForAnimationEvent();
        }
        public override void OnUpdate()
        {
            attackAmount = controller.BuffStatus(BuffType.Power,controller.MonsterStatus.AttackAmount);
            controller.CheckParesis_Monster(controller.animator);
            //Debug.Log($"[アニメ状態] name: {state.fullPathHash}, time: {state.normalizedTime}, looping: {state.loop}");
            if (!isAttacking)
            {
                isAttacking = true;
                Attack();

            }
            if (controller.MonsterStatus.MonsterMoveType == MonsterMoveType.Fly) LookToTarget();
            MoveToChaseState();
        }

        public override void OnExit()
        {
            controller.animator.speed = 1.0f;
            controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);
            cts?.Cancel();
            cts?.Dispose();
        }

        protected virtual async UniTask Attack_Simple()
        {
            try
            {
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName), cancellationToken: cts.Token);
                controller.animator.speed = 1.0f;
                Debug.Log(target.gameObject.name);
                //var animDuration = clipLength * animationSpeed;
                var startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    var now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - startNormalizeTime >= attackEndNomTime;
                });
                //Func<bool> waitEnd = (() =>
                //{
                //    var now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                //    return now - startNormalizeTime >= 1.0f;
                //});
                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);//,);
                if (target != null && target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
                {
                    Debug.Log($"{controller.gameObject.name}のアタック");
                    unitDamagable.Damage(attackAmount);
                    EffectManager.Instance.hitEffect.GenerateHitEffect(target);
                }
                //await UniTask.WaitUntil(waitEnd, cancellationToken: cts.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
               if(cts.IsCancellationRequested) isAttacking = false;
            }
        }
        void MoveToChaseState()
        {
            Debug.Log(target.gameObject.name);
            var canAttack = false;
            var targetPos = Vector3.zero;
            var isDead = target.isDead;

            //if (target is TowerControlller)
            //{
                var collider = target.GetComponent<Collider>();
                targetPos = collider.ClosestPoint(controller.transform.position);
                targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                canAttack = (targetPos - controller.transform.position).magnitude <= attackRange && !isDead;// && !isDead;
            //}
            //else if (target is IMonster || target is IPlayer)
            //{
            //    targetPos = target.transform.position;
            //    targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
            //    canAttack = (targetPos - controller.transform.position).magnitude <= attackRange && !isDead;// 
            //}
                Debug.Log($"[距離チェック] 距離: {canAttack}, 射程: {controller.MonsterStatus.AttackRange}");
            if (!canAttack)
            {
                CancelAttack();
            }
        }
        
        float GetAttackRange()
        {
            if (controller.MonsterStatus == null) return default;
            var unitMoveType = controller.MonsterStatus.MonsterMoveType;
            var baseRange = controller.MonsterStatus.AttackRange;
            return unitMoveType == MonsterMoveType.Walk ? baseRange * 1.3f
                 : unitMoveType == MonsterMoveType.Fly ? baseRange * 1.2f : baseRange;
        }
        void LookToTarget()
        {
            Renderer renderer = null;
            if (target.TryGetComponent(out TowerControlller tower)) renderer = tower.MyMeshes[0];
            else if (target is IPlayer || target is IMonster) renderer = target.MySkinnedMeshes[0];
            else return;
            var lookPos = renderer.bounds.center;
            var myCriterionPos = controller.MySkinnedMeshes[0].bounds.center;
            var direction = lookPos - myCriterionPos;
            var rotation = Quaternion.LookRotation(direction);
            controller.transform.rotation = rotation;
            
        }
        
        void CancelAttack()
        {
            Debug.Log("敵が範囲外に行きました");
            nextState = controller.ChaseState;
            isAttacking = false;
            controller.ChangeState(nextState);
            //controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);//ここに必要かもしれないから必要だったらコメントアウト消して
        }

        //void CheckParesis()
        //{
        //    var paresis = controller.statusCondition.Paresis.isActive;
        //    var currentAnimatorSpeed = controller.animator.speed;
        //    var notZeroSpeed = currentAnimatorSpeed != 0;
        //    if (paresis && notZeroSpeed)
        //    {
        //        Debug.Log("麻痺中です");
        //        controller.animator.speed = 0.5f;
        //    }
        //    else if (!paresis && notZeroSpeed)
        //    {
        //        Debug.Log("麻痺が治りました");
        //        controller.animator.speed = 1.0f;
        //    }
        //}

        public void Attack()
        {
            if (controller.MonsterStatus.AttackType == AttackType.Simple) Attack_Simple().Forget();
        }

        public async void StopAnimFromEvent()
        {
            Debug.Log("イベントが呼ばれました");
            controller.animator.speed = 0f;
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: cts.Token);
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            finally 
            {
                isAttacking = false;
            }
        }

        void ChangeClipForAnimationEvent()
        {
            var clipName = controller.MonsterAnimPar.attackAnimClipName;
            var attackMotionClip = AnimatorClipGeter.GetAnimationClip(controller.animator,clipName);
            var eventSetTime = clipLength - 0.01f;

            var originalController = controller.animator.runtimeAnimatorController;
            var overrideController = new AnimatorOverrideController(originalController);
            controller.animator.runtimeAnimatorController = overrideController;

            var newClip = UnityEngine.Object.Instantiate(attackMotionClip);
            AnimationEvent animationEvent = new AnimationEvent();
            animationEvent.functionName = "StopAnimation_AttackState";
            newClip.name = clipName + "_PlusEvent";
            animationEvent.time = eventSetTime;
            newClip.AddEvent(animationEvent);

            overrideController[attackMotionClip.name] = newClip;
            isSettedEventClip = true;
        }
    }

}

