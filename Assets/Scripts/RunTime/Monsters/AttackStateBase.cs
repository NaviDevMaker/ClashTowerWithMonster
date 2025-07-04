using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;
using UnityEngine.Events;

namespace Game.Monsters
{
    public class AttackStateBase<T> : StateMachineBase<T>, IUnitAttack where T : MonsterControllerBase<T>
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
        bool isSettedEventClip = false;
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
            controller.animator.SetBool(controller.MonsterAnimPar.Attack, true);
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
             LookToTarget();//if (controller.MonsterStatus.MonsterMoveType == MonsterMoveType.Fly)
            MoveToChaseState();//if(!isWaitingLeftTime)
        }

        public override void OnExit()
        {
            //controller.animator.speed = 1.0f;
            //cts?.Cancel();
            cts?.Dispose();

            controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);
        }

        protected virtual async UniTask Attack_Simple(UnityAction statusConditionAttack = null)
        {
            float startNormalizeTime = 0f;
            float now = 0f;
            try
            {
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
                if (target == null) return;
                AddDamageToTarget();
                if (statusConditionAttack != null) statusConditionAttack?.Invoke();
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            finally
            {
                if (cts.IsCancellationRequested)
                {
                    leftLengthTime = Mathf.Max(0f, (now - startNormalizeTime) * clipLength) / stateAnimSpeed;
                    isAttacking = false;
                }
                else leftLengthTime = 0f;
            }
        }

        protected virtual async UniTask Attack_Long()
        {
            await UniTask.CompletedTask;
        }
        void AddDamageToTarget()
        {
            if (target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                Debug.Log($"{controller.gameObject.name}のアタック");
                unitDamagable.Damage(attackAmount);
                EffectManager.Instance.hitEffect.GenerateHitEffect(target);
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
            var closestPos = collider.ClosestPoint(controller.transform.position);
            //targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);

            targetPos = PositionGetter.GetFlatPos(closestPos);
            var myPos = PositionGetter.GetFlatPos(controller.transform.position);
            canAttack = (targetPos - myPos).magnitude <= attackRange && !isDead;// && !isDead;
           
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
        void LookToTarget()
        {
            Renderer renderer = target.BodyMesh;
            //if (target.TryGetComponent(out TowerControlller tower)) renderer = tower.MyMeshes[0];
            //else if (target is IPlayer || target is IMonster) renderer = target.MySkinnedMeshes[0];
            if(renderer == null) return;
            var lookPos = renderer.bounds.center;
            var myCriterionPos = controller.BodyMesh.bounds.center;
            var direction = lookPos - myCriterionPos;
            if (controller.moveType == MoveType.Walk) direction.y = 0f;
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
            }
            catch(OperationCanceledException)
            {
                return;
            }
            finally
            {
                isWaitingLeftTime = false;
            }        
            if(ContinueAttackState())
            {
                Debug.Log("新しい敵が見つかりました");
                controller.animator.Play(controller.MonsterAnimPar.attackAnimClipName);
                cts = new CancellationTokenSource();
                isAttacking = true;
                Attack();
                return;
            }
            nextState = controller.ChaseState;
            controller.ChangeState(nextState);
            //controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);//ここに必要かもしれないから必要だったらコメントアウト消して
        }

        bool ContinueAttackState()
        {
            var monsterAttackType = controller.MonsterStatus.MonsterAttackType;
            var serchRange = monsterAttackType == MonsterAttackType.ToEveryThing ? controller.MonsterStatus.ChaseRange
               : monsterAttackType == MonsterAttackType.OnlyBuilding ? controller.MonsterStatus.AttackRange : 0f;


            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject, controller.MonsterStatus.ChaseRange);

            var mySide = controller.Side;
            var myType = controller.moveType;
            var effecttiveSide = myType switch
            {
                MoveType.Walk => MoveType.Walk,
                MoveType.Fly => MoveType.Fly | MoveType.Walk,
                _ => default
            };

            var filterdArray = monsterAttackType switch
            {
                MonsterAttackType.ToEveryThing => sortedArray.Where(cmp =>

                {
                    var enemySide = cmp.Side;
                    var isDead = cmp.isDead;
                    var moveType = cmp.moveType;
                    if (cmp.TryGetComponent<ISummonbable>(out var summonbable))
                    {
                        var isSummoned = summonbable.isSummoned;
                        return enemySide != mySide && !isDead
                          && (moveType & effecttiveSide) != 0 && isSummoned;// 
                    }
                    return enemySide != mySide && !isDead
                            && (moveType & effecttiveSide) != 0;// 
                }).ToArray(),

                MonsterAttackType.OnlyBuilding => sortedArray.Where(cmp =>
                {
                    var enemySide = cmp.Side;
                    var isDead = cmp.isDead;
                    var building = cmp is IBuilding;
                    return enemySide != mySide && !isDead && building;
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
            if (controller.MonsterStatus.AttackType == AttackType.Simple) Attack_Simple().Forget();
            else if(controller.MonsterStatus.AttackType == AttackType.Long) Attack_Long().Forget();

        }

        public virtual async void StopAnimFromEvent()
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

