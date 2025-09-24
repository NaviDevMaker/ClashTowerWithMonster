using Cysharp.Threading.Tasks;
using Game.Monsters.Salamander;
using NUnit.Framework.Internal.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.KingDragon
{
    public class AttackState : StateMachineBase<KingDragonController>, ILongDistanceAction<KingDragonController>

    {
        public AttackState(KingDragonController controller) : base(controller) { }
        public class AnimationInfo
        {
            public float simpleAttackAnimSpeed { get; private set; } = 0.5f;
            public float longAttackAnimSpeed { get; private set; } = 0.5f;
            public float simpleAttackClipLength { get; private set; }
            public float longAttackClipLength { get; private set; }
            public readonly int simpleAttackEndFrame = 15;
            public readonly int longAttackEndFrame = 21;
            public float simpleAttackEndNomTime { get; private set; } = 0;
            public float longAttackEndNomTime { get; private set; } = 0;
            public float interval { get; private set; } = 2f;
            public AnimationInfo(KingDragonController controller) => SetAnimationInfo(controller);
            void SetAnimationInfo(KingDragonController controller)
            {
                var animPar = controller.KingDragonAnimPar;
                var simpleAt = animPar.attackAnimClipName;
                var longAt = animPar.attack_2AnimClipName;
                var simpleClip = controller.animator.GetAnimationClip(simpleAt);
                var longClip = controller.animator.GetAnimationClip(longAt);
                var maxFrame_Simple = GetMaxFrame(simpleClip, out var simpleLen);
                var maxFrame_Long = GetMaxFrame(longClip, out var longAttackLen);
                simpleAttackClipLength = simpleLen;
                longAttackClipLength = longAttackLen;
                simpleAttackEndNomTime = (float)simpleAttackEndFrame / (float)maxFrame_Simple;
                longAttackEndNomTime = (float)longAttackEndFrame / (float)maxFrame_Long;

                Debug.Log($"シンプル{simpleAttackClipLength},ロング{longAttackClipLength}" +
                    $"{simpleAttackEndNomTime},{longAttackEndNomTime}");

            }
            int GetMaxFrame(AnimationClip animationClip, out float length)
            {
                var cliplength = animationClip.length;
                var frameRate = animationClip.frameRate;
                length = cliplength;
                return Mathf.RoundToInt(frameRate * cliplength);
            }
        }

        public AnimationInfo animationInfo;
        public CancellationTokenSource cts { get; set; } = new CancellationTokenSource();
        public enum CurrentAttackType
        {
            Simple,
            Long
        }
        public CurrentAttackType currentAttackType { get; private set; } = CurrentAttackType.Simple;
        public UnitBase targetEnemy { get; set; }
        public int attackAmount = 0;

        public float leftLengthTime = 0f;
        public float startNormalizeTime = 0f;
        public bool isAttacking = false;
        public bool isWaitingLeftTime = false;
        public bool isContineAttack = false;
        public bool canChangeAttackType = false;
        public bool isShotingFire = false;
        public bool isChecking = false;
        bool isInitialized = false;
        public bool isInterval { get; private set; } = false;

        Dictionary<CurrentAttackType, UnityAction> attackActionDic = new Dictionary<CurrentAttackType, UnityAction>();
        public StateMachineBase<KingDragonController> _nextState => nextState;
        public override void OnEnter()
        {
            nextState = controller.SearchState;
            if (!isInitialized)
            {
                Initialized();
                isInitialized = true;
            }
        }
        public override void OnExit()
        {
            cts?.Dispose();
            controller.animator.speed = 1.0f;
            controller.animator.SetBool(controller.KingDragonAnimPar.Attack_Hash, false);
            controller.animator.SetBool(controller.KingDragonAnimPar.Attack2_Hash, false);
        }
        public override void OnUpdate()
        {
            Debug.Log($"{isAttacking},{targetEnemy}");
            if (!isAttacking && !isWaitingLeftTime && targetEnemy != null)
            {
                isAttacking = true;
                Attack();
            }
            if (isInterval || isContineAttack) controller.kingDragonMethod.LookToTarget();
            if (isChecking) return;
            if(!CheckAttackable()) controller.kingDragonMethod.CancelAttack();
        }
        bool CheckAttackable()
        {          
            if(targetEnemy == null) return false;
            var targetPos = Vector3.zero;
            var isDead = targetEnemy.isDead;
            var isTransparent = targetEnemy.statusCondition.Transparent.isActive;
            var isNonTarget = targetEnemy.statusCondition.NonTarget.isActive;
            var collider = targetEnemy.GetComponent<Collider>();
            var closestPos = collider.ClosestPoint(controller.transform.position);
            var effectiveMoveSide = currentAttackType switch
            {
                CurrentAttackType.Simple => MoveType.Walk,
                CurrentAttackType.Long => MoveType.Fly | MoveType.Walk,
                _ => default
            };
            var targetMoveType = targetEnemy.moveType;
            targetPos = PositionGetter.GetFlatPos(closestPos);
            var myPos = PositionGetter.GetFlatPos(controller.transform.position);
            var currentNom = controller.animator.GetCurrentNormalizedTime(startNormalizeTime);
            var canAttack = currentAttackType switch
            {
                CurrentAttackType.Simple => (targetPos - myPos).magnitude <= controller.KingDragonStatus.AttackSimpleRange
                                             && !isDead && !isTransparent && !isNonTarget
                                             && (effectiveMoveSide & targetMoveType) != 0,
                CurrentAttackType.Long => !isShotingFire ? (targetPos - myPos).magnitude <= controller.KingDragonStatus.AttackLongRange
                                             && !isDead && !isTransparent && !isNonTarget
                                             && (effectiveMoveSide & targetMoveType) != 0
                                             : !cts.IsCancellationRequested,
                _ => default
            };
            return canAttack;
        }
        public void Attack()
        {
            Debug.Log("攻撃を開始します");
            if (canChangeAttackType)
            {
                ChangeCurrentAttackType(currentAttackType);
                var newTarget = controller.kingDragonMethod.SetTarget(currentAttackType);
                if (newTarget == null && currentAttackType == CurrentAttackType.Long) return;
                else if(newTarget == null && currentAttackType == CurrentAttackType.Simple)
                {
                    ChangeCurrentAttackType(currentAttackType);
                    newTarget = controller.kingDragonMethod.SetTarget(currentAttackType);
                    if (newTarget == null) return;
                }
                targetEnemy = newTarget;
                Debug.Log($"新しいターゲット{targetEnemy},{currentAttackType}");
            }
            var action = attackActionDic[currentAttackType];
            action.Invoke();
        }
        void ChangeClipsForEvent()
        {
            var originalController = controller.animator.runtimeAnimatorController;
            var overrideController = new AnimatorOverrideController(originalController);
            controller.animator.runtimeAnimatorController = overrideController;
            var animPar = controller.KingDragonAnimPar;
            controller.animator.ChangeClipForAnimationEvent(overrideController, animPar.attackAnimClipName,
                                                            animationInfo.simpleAttackClipLength);
            controller.animator.ChangeClipForAnimationEvent(overrideController, animPar.attack_2AnimClipName,
                                                            animationInfo.longAttackClipLength);
        }
        void Initialized()
        {

            var simpleArg = new SimpleAttackArguments
            {
                getTargets = () => targetEnemy != null ? new List<UnitBase> { targetEnemy }
                                  : Enumerable.Empty<UnitBase>().ToList()
            };

            var longArg = new LongAttackArguments<KingDragonController>
            {
                getNextMover = GetNextMover,
                moveAction = NextMoverAction,
                repeatCount = controller.repeatCount,
            };

            attackActionDic[CurrentAttackType.Simple] = () => controller.kingDragonMethod.Attack_Simple(simpleArg).Forget();
            attackActionDic[CurrentAttackType.Long] = () => controller.kingDragonMethod.Attack_Long(longArg).Forget();
            animationInfo = new AnimationInfo(controller);
            Debug.Log($"{animationInfo.simpleAttackClipLength},{animationInfo.longAttackClipLength}");
            ChangeClipsForEvent();
        }
        public void StopAnimFromEvent() => controller.StopAnimation(animationInfo.interval);
        public LongDistanceAttack<KingDragonController> GetNextMover()
        {
            foreach (var mover in controller.movers)
            {
                if (mover is KingDragonFireMover fireMover)
                {
                    if (!fireMover.gameObject.activeInHierarchy && !fireMover.IsProcessingTask) return mover;
                }
            }
            return null;
        }
        public void NextMoverAction(LongDistanceAttack<KingDragonController> nextMover)
        {
            var isDead = targetEnemy.isDead;
            var isTransparent = targetEnemy.statusCondition.Transparent.isActive;
            var isNonTarget = targetEnemy.statusCondition.NonTarget.isActive;
            if (nextMover != null)
            {

                var shotableCurrentEnemy = !isDead && !isTransparent && !isNonTarget;
                if (targetEnemy == null || !shotableCurrentEnemy) targetEnemy = controller.kingDragonMethod.SetTarget(currentAttackType);
                if (targetEnemy == null) return;
                nextMover.target = targetEnemy;
                nextMover.gameObject.SetActive(true);
                nextMover.Move();
                Debug.Log("打たれました");
            }
        }
        public void ChangeCurrentAttackType(CurrentAttackType attackType)
        {
            var nextAttackType = attackType switch
            {
                CurrentAttackType.Simple => CurrentAttackType.Long,
                CurrentAttackType.Long => CurrentAttackType.Simple,
                _ => default
            };
            currentAttackType = nextAttackType;
            canChangeAttackType = false;
        }
    }
}
