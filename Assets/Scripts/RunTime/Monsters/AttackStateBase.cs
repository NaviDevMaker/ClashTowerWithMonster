using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;

public interface IAttackState
{
    bool isInterval { get;}
}

namespace Game.Monsters
{
    public class AttackStateBase<T> : StateMachineBase<T>, IUnitAttack,IAttackState where T : MonsterControllerBase<T>
    {
        public AttackStateBase(T controller) : base(controller) { }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        //Vector3 towerTargetPos;
        public UnitBase target;
        protected int attackAmount = 0;
        protected float stateAnimSpeed { get; private set; } = 0f;
        float attackRange = 0f;
        protected CancellationTokenSource cts;

        public int maxFrame = 0;
        public int attackEndFrame = 0;
        public float attackEndNomTime = 0f;
        public float interval = 0f;

        protected float leftLengthTime = 0f;
        protected bool isAttacking = false;
        bool isWaitingLeftTime = false;
        bool isContineAttack = false;
        bool isSettedEventClip = false;
        public bool isInterval { get; private set; } = false;
        public override void OnEnter()
        {
            SetUp();
            if (!isSettedEventClip) ChangeClipForAnimationEvent();//
        }

        protected void SetUp()
        {
            attackAmount = controller.statusCondition != null ? controller.BuffStatus(BuffType.Power, controller.MonsterStatus.AttackAmount)
               : controller.MonsterStatus.AttackAmount;
            Debug.Log("Attackに入りました");
            if (attackRange == 0f)
            {
                stateAnimSpeed = controller.MonsterStatus.AnimaSpeedInfo.AttackStateAnimSpeed;
                attackRange = GetAttackRange();
            }
            cts = new CancellationTokenSource();
            controller.animator.SetBool(controller.MonsterAnimPar.Attack_Hash, true);
            if (clipLength == 0)
            {
                clipLength = controller.GetAnimClipLength();
            }
        }
        public override void OnUpdate()
        {
            
            Debug.Log($"アニメーターのスピー度は{controller.animator.speed}");
            Debug.Log($"{isWaitingLeftTime},{isAttacking}");
            attackAmount = controller.statusCondition != null ? controller.BuffStatus(BuffType.Power, controller.MonsterStatus.AttackAmount)
                : controller.MonsterStatus.AttackAmount;
            controller.CheckParesis_Monster(controller.animator);
            //Debug.Log($"[アニメ状態] name: {state.fullPathHash}, time: {state.normalizedTime}, looping: {state.loop}");
            if (!isAttacking && !isWaitingLeftTime)
            {
                isAttacking = true;
                Attack();
            }
            if((isInterval || isContineAttack) && !controller.statusCondition.Freeze.isActive) LookToTarget();
            MoveToChaseState();
        }

        public override void OnExit()
        {
            controller.animator.speed = 1.0f;//凍ってるときに死んだ時用
            //cts?.Cancel();
            cts?.Dispose();

            controller.animator.SetBool(controller.MonsterAnimPar.Attack_Hash, false);
        }

        protected virtual async UniTask Attack_Generic(Func<List<UnitBase>> getTargets
            ,UnityAction<UnitBase> specialEffectAttack = null,UnityAction continuousAttack = null)
        {
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName))
            {
                controller.animator.Play(controller.MonsterAnimPar.attackAnimClipName);
            }

            float startNormalizeTime = 0f;
            float now = 0f;
            try
            {
                if (!controller.statusCondition.Freeze.isActive) LookToTarget();
                await UniTask.WaitUntil(() =>
                {
                    if (controller.isDead) return false;
                    return controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName);
                }, cancellationToken: cts.Token);
                controller.animator.speed = 1.0f;
                Debug.Log(target.gameObject.name);
                startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    if (controller.isDead) return true;
                    now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - startNormalizeTime >= attackEndNomTime;
                });
               
                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);
                var currentTargets = getTargets();
                if(currentTargets.Count == 0 || target == null) return;
                //currentTargets.Add(target);
                currentTargets.ForEach(target =>
                { 
                    AddDamageToTarget(target);
                    if (specialEffectAttack != null) specialEffectAttack?.Invoke(target);
                    if (continuousAttack != null) continuousAttack?.Invoke();
                });                 
            }
            catch (OperationCanceledException)
            {
                var elapsedTime = (now - startNormalizeTime) * clipLength;
                leftLengthTime = Mathf.Max(0f, clipLength - elapsedTime / stateAnimSpeed);
                isAttacking = false;
            }
            catch (ObjectDisposedException) { }
            finally { }
            leftLengthTime = 0f;
        }

        protected virtual async UniTask Attack_Long()
        {
            await UniTask.CompletedTask;
        }
        protected void AddDamageToTarget(UnitBase currentTarget)
        {
            if (currentTarget.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                Debug.Log($"{controller.gameObject.name}のアタック");
                unitDamagable.Damage(attackAmount);
                EffectManager.Instance.hitEffect.GenerateHitEffect(currentTarget);
            }
        }
        void MoveToChaseState()
        {
            Debug.Log(target.gameObject.name);
            var canAttack = false;
            var targetPos = Vector3.zero;
            var isDead = target.isDead;
            var collider = target.GetComponent<Collider>();
            var closestPos = collider.ClosestPoint(controller.transform.position);

            targetPos = PositionGetter.GetFlatPos(closestPos);
            var myPos = PositionGetter.GetFlatPos(controller.transform.position);
            var isConfused = controller.statusCondition.Confusion.isActive;
            var targetSide = target.GetUnitSide(controller.ownerID);
            var effectiveSide = isConfused switch
            {
                true => Side.PlayerSide | Side.EnemySide,
                false => Side.EnemySide,
            };

            canAttack = (targetPos - myPos).magnitude <= attackRange && !isDead && (targetSide & effectiveSide) != 0;// && !isDead;         
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
            if(controller.MonsterStatus.MonsterAttackType == MonsterAttackType.OnlyBuilding) return baseRange;
            return unitMoveType == MonsterMoveType.Walk ? baseRange * 1.15f
                 : unitMoveType == MonsterMoveType.Fly ? baseRange * 1.2f : baseRange;
        }
        protected void LookToTarget()
        {
            Debug.Log("キャラの方向を向きます");
            if (target == null) return;
            Renderer renderer = target.BodyMesh;
            if(renderer == null) return;
            var lookPos = renderer.bounds.center;
            var myCriterionPos = controller.BodyMesh.bounds.center;
            var direction = lookPos - myCriterionPos;       
            direction.y = 0f;
            var rotation = Quaternion.LookRotation(direction);      
            controller.transform.rotation = rotation;         
        }     
        async void CancelAttack()
        {
            Debug.Log("敵が範囲外に行きました");
            cts?.Cancel();
            isAttacking = false;
            isWaitingLeftTime = true;
            controller.animator.speed = 1.0f;
            controller.animator.Play("Idle");

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(leftLengthTime), cancellationToken: controller.GetCancellationTokenOnDestroy());
                if (ContinueAttackState())
                {
                    isContineAttack = true;
                    var continueAttackInterval = interval - leftLengthTime;
                    await UniTask.Delay(TimeSpan.FromSeconds(continueAttackInterval), cancellationToken: controller.GetCancellationTokenOnDestroy());
                    controller.animator.Play(controller.MonsterAnimPar.attackAnimClipName);
                    cts = new CancellationTokenSource();
                    isAttacking = true;
                    Attack();
                    return;
                }
            }
            catch(OperationCanceledException)
            {
                return;
            }
            finally
            {
                isWaitingLeftTime = false;
                isContineAttack = false;
            }        
           
            nextState = controller.ChaseState;
            controller.ChangeState(nextState);
            //controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);//ここに必要かもしれないから必要だったらコメントアウト消して
        }

        bool ContinueAttackState()
        {
            var monsterAttackType = controller.MonsterStatus.MonsterAttackType;

            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject, controller.MonsterStatus.ChaseRange);

            var myType = controller.moveType;
            var effectiveMoveSide = myType switch
            {
                MoveType.Walk => MoveType.Walk,
                MoveType.Fly => MoveType.Fly | MoveType.Walk,
                _ => default
            };

            var filterdArray = monsterAttackType switch
            {
                MonsterAttackType.RelyOnMoveType => sortedArray.Where(cmp =>

                {
                    var enemySide = cmp.GetUnitSide(controller.ownerID);
                    var isDead = cmp.isDead;
                    var moveType = cmp.moveType;
                    var isConfused = controller.statusCondition.Confusion.isActive;
                    var effectiveSide = isConfused switch
                    {
                       true => Side.PlayerSide | Side.EnemySide,
                       false => Side.EnemySide,
                    };

                    if (cmp.TryGetComponent<ISummonbable>(out var summonbable))
                    {
                        var isSummoned = summonbable.isSummoned;
                        return (enemySide & effectiveSide) != 0 && !isDead
                          && (moveType & effectiveMoveSide) != 0 && isSummoned;// 
                    }
                    return (enemySide & effectiveSide) != 0 && !isDead
                            && (moveType & effectiveMoveSide) != 0;// 
                }).ToArray(),

                MonsterAttackType.OnlyBuilding => sortedArray.Where(cmp =>
                {
                    var enemySide = cmp.GetUnitSide(controller.ownerID);
                    var isDead = cmp.isDead;
                    var building = cmp is IBuilding;
                    return enemySide != Side.PlayerSide && !isDead && building;
                }).ToArray(),

                MonsterAttackType.ToEveryThing => sortedArray.Where(cmp =>
                {
                    var enemySide = cmp.GetUnitSide(controller.ownerID);
                    var isDead = cmp.isDead;
                    var isConfused = controller.statusCondition.Confusion.isActive;
                    var effectiveSide = isConfused switch
                    {
                        true => Side.PlayerSide | Side.EnemySide,
                        false => Side.EnemySide,
                    };

                    if (cmp.TryGetComponent<ISummonbable>(out var summonbable))
                    {
                        var isSummoned = summonbable.isSummoned;
                        return (enemySide & effectiveSide) != 0 && !isDead
                               && isSummoned;// 
                    }
                    return (enemySide & effectiveSide) != 0 && !isDead;
                }).ToArray(),   
                _ => default
            };
           
            if(filterdArray.Length > 0)
            {
                var newTarget = filterdArray[0];
                if (target == newTarget) return false;
                target = newTarget;
                return true;
            }

            return false;
        }
       
        public void Attack()
        {
            if (controller.MonsterStatus.AttackType == AttackType.Simple || controller.MonsterStatus.AttackType == AttackType.Continuous)
            {
                Attack_Generic(() => target != null ? new List<UnitBase> { target } : Enumerable.Empty<UnitBase>().ToList()).Forget();
            }
            else if(controller.MonsterStatus.AttackType == AttackType.Range)
            {
                Attack_Generic(() => controller.GetUnitInWeponRange().Invoke()).Forget();
            }
            else if (controller.MonsterStatus.AttackType == AttackType.Long) Attack_Long().Forget();
        }
        public virtual async void StopAnimFromEvent()
        {
            Debug.Log("イベントが呼ばれました");
            isInterval = true;
            controller.animator.speed = 0f;
            try
            {
                var stopAnimInterval = controller.animator.speed != 0f ? interval / controller.animator.speed :interval;
                await UniTask.Delay(TimeSpan.FromSeconds(stopAnimInterval), cancellationToken: cts.Token);
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
            finally 
            {
                isInterval = false;
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

