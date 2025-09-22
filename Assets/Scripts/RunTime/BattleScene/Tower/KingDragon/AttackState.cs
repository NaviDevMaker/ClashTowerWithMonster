using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.KingDragon
{
    public class AttackState : StateMachineBase<KingDragonController>
    {
        public AttackState(KingDragonController controller) : base(controller) { }
        public class AnimationInfo
        {
            public float simpleAttackAnimSpeed { get; private set; } = 0.5f;
            public float longAttackAnimSpeed { get; private set; } = 0.5f;
            public float simpleAttackClipLength { get; private set;}
            public float longAttackClipLength { get; private set;}
            public readonly int simpleAttackEndFrame  = 15;
            public readonly int longAttackEndFrame = 21;
            public float simpleAttackEndNomTime { get; private set;} = 0;
            public float longAttackEndNomTime { get; private set;} = 0;
            public float interval { get; private set;} = 0f;
            public AnimationInfo(KingDragonController controller) => SetAnimationInfo(controller);
            void SetAnimationInfo(KingDragonController controller)
            {
                var animPar = controller.KingDragonAnimPar;
                var simpleAt = animPar.attackAnimClipName;
                var longAt = animPar.attack_2AnimClipName;
                var simpleClip = controller.animator.GetAnimationClip(simpleAt);
                var longClip = controller.animator.GetAnimationClip(longAt);
                var maxFrame_Simple = GetMaxFrame(simpleClip, out float simpleAttackClipLength);
                var maxFrame_Long = GetMaxFrame(longClip, out float longAttackClipLength);

                simpleAttackEndNomTime = (float)simpleAttackEndFrame / (float)maxFrame_Simple;
                longAttackEndNomTime = (float)longAttackEndFrame / (float)maxFrame_Long;
            }
            int GetMaxFrame(AnimationClip animationClip,out float length)
            {
                var cliplength = animationClip.length;
                var frameRate = animationClip.frameRate;
                length = cliplength;
                return Mathf.RoundToInt(frameRate * cliplength);
            }
        }

        KingAttackMethod attackMethod = null;
        public AnimationInfo animationInfo;
        public CancellationTokenSource cts { get; set;} = new CancellationTokenSource();
        public enum CurrentAttackType
        {
            Simple,
            Long
        }
        public CurrentAttackType currentAttackType = CurrentAttackType.Simple;
        public UnitBase targetEnemy { get; set;}
        public int attackAmount = 0;
      
        public float leftLengthTime = 0f;
        public float startNormalizeTime  = 0f;
        public bool isAttacking = false;
        public bool isWaitingLeftTime = false;
        public bool isContineAttack = false;
        bool isInitialized  = false;
        public bool canChangeAttackType = false;
        public bool isInterval { get; private set; } = false;
        
        Dictionary<CurrentAttackType,UnityAction> attackActionDic = new Dictionary<CurrentAttackType,UnityAction>();
        public StateMachineBase<KingDragonController> _nextState => nextState;
        public override void OnEnter()
        {
            nextState = controller.SearchState;
            if (!isInitialized)
            {
                var arg = new SimpleAttackArguments();
                var lomgArg = new LongAttackArguments<KingDragonController>();
                attackMethod = new KingAttackMethod(controller);
                attackActionDic[CurrentAttackType.Simple] = () => attackMethod.Attack_Simple(arg).Forget();
                attackActionDic[CurrentAttackType.Long] = () => attackMethod.Attack_Long(lomgArg).Forget();
                animationInfo = new AnimationInfo(controller);
                isInitialized = true;
            }           
        }
        public override void OnExit()
        {
            cts?.Dispose();
            controller.animator.SetBool(controller.KingDragonAnimPar.Attack_Hash, false);
            controller.animator.SetBool(controller.KingDragonAnimPar.Attack2_Hash, false);
        }
        public override void OnUpdate()
        {
            if (!isAttacking && !isWaitingLeftTime)
            {
                isAttacking = true;
                Attack();
            }
            if (isInterval || isContineAttack) attackMethod.LookToTarget();
            if (!CheckAttackable()) attackMethod.CancelAttack();
        }
        bool CheckAttackable()
        {
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
                CurrentAttackType.Long  => (animationInfo.longAttackEndNomTime <= currentNom),
                _ => default
            };                     
            return canAttack;
        }
        public void Attack()
        {
            //currentAttackType = currentAttackType switch
            //{
            //    CurrentAttackType.Simple => CurrentAttackType.Long,
            //    CurrentAttackType.Long => CurrentAttackType.Simple,
            //    _=> default
            //};
            var action = attackActionDic[currentAttackType];
            action.Invoke();
        }
    }
}
