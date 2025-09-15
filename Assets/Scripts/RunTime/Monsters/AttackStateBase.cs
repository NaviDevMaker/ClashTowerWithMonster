using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Linq;
using UnityEngine.Events;
using System.Collections.Generic;
using Game.Monsters;
using static UnityEngine.Rendering.DebugUI.Table;
using Unity.VisualScripting;

public interface IAttackState
{
    bool isInterval { get;}
}

namespace Game.Monsters
{
    public class AttackStateBase<T> : StateMachineBase<T>, IUnitAttack,IAttackState where T : MonsterControllerBase<T>
    {
        public interface ILongDistanceAction
        {
            LongDistanceAttack<T> GetNextMover();
            void NextMoverAction(LongDistanceAttack<T> nextMover);
        }
        protected class SimpleAttackArguments
        {
            public Func<List<UnitBase>> getTargets;//�^�[�Q�b�g�̎擾
            public UnityAction<UnitBase> specialEffectAttack = null;//�U����̌ʂ̓G�ɑ΂��Ă̓��ʂȏ����i���̍U�����HP�񕜂Ȃǁj
            public UnityAction attackEffectAction = null;//�_���[�W��^����t���[���ɂȂ������ɏo���G�t�F�N�g�̃A�N�V����(�S�[�����̉��Ȃ�)
            public UnityAction attackEndAction = null;//�U���I���ɂ��鏈��
            public UnityAction<Func<float>> actionByLeftAttackLength = null;//�����U���J�n���̎��Ԃ��֌W����悤�ȏ����̎�
            public float repeatCount = 0f;//�U���񐔁i�T�C�N���v�X��i�[�K�E�B�U�[�h�̑��i�U���̉񐔁j
        }
        protected class LongAttackArguments
        {
            public Func<LongDistanceAttack<T>> getNextMover;
            public UnityAction<LongDistanceAttack<T>> moveAction;
            public UnityAction attackEffectAction = null;
            public UnityAction attackEndAction = null;
            public int repeatCount = 0;
        }

        public AttackStateBase(T controller) : base(controller) { }
        
        public UnitBase target;
        protected int attackAmount = 0;
        protected float stateAnimSpeed { get; private set; } = 0f;
        protected float attackRange { get; private set;} = 0f;
        protected CancellationTokenSource cts;
        //�L�����Z�������͎̂������g�̃I�u�W�F�N�g���j�󂳂ꂽ�Ƃ��A���͓G��CheckAttackable()��false�̎�
        //�����OnExit��Dispose(���񂾂Ƃ�)��Attack����Dispose��catch�����邩������L�����Z���Ɠ�����Repeat�n�̃A�^�b�N�̎���
     �@ //�U���͌p�����Ȃ�����
       �@//�G�t�F�N�g�n��attackEndAction��finally�ŌĂяo������G�t�F�N�g�̌p�������Ȃ�����
        public int maxFrame = 0;
        public int attackEndFrame = 0;
        public float attackEndNomTime = 0f;
        public float interval = 0f;
        protected float startNormalizeTime = 0f;

        protected float leftLengthTime = 0f;
        protected bool isAttacking = false;
        bool isWaitingLeftTime = false;
        bool isContineAttack = false;
        public bool isSettedEventClip { get; private set;} = false;
        public bool isInterval { get; private set; } = false;
        public bool _isAbsorbed = false;//����SO�������t���N�V�����ŎQ�Ƃ�
        
        public override void OnEnter()
        {
            SetUp();
            if (!isSettedEventClip) ChangeClipForAnimationEvent();//
        }
        protected void SetUp()
        {
            attackAmount = controller.statusCondition != null ? controller.BuffStatus(BuffType.Power, controller.MonsterStatus.AttackAmount)
               : controller.MonsterStatus.AttackAmount;
            Debug.Log("Attack�ɓ���܂���");
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
            
            Debug.Log($"�A�j���[�^�[�̃X�s�[�x��{controller.animator.speed}");
            Debug.Log($"{isWaitingLeftTime},{isAttacking}");
            attackAmount = controller.statusCondition != null ? controller.BuffStatus(BuffType.Power, controller.MonsterStatus.AttackAmount)
                : controller.MonsterStatus.AttackAmount;
            controller.CheckParesis_Monster(controller.animator);
            //����z���Ƃ��̊֌W�Ȃ����
            //controller.MonsterStatus.AttackStateUpdateMethod(controller);
            //Debug.Log($"[�A�j�����] name: {state.fullPathHash}, elapsedTime: {state.normalizedTime}, looping: {state.loop}");
            if (!isAttacking && !isWaitingLeftTime)
            {
                isAttacking = true;
                Attack();
            }
            if((isInterval || isContineAttack) && !controller.statusCondition.Freeze.isActive) LookToTarget();
            if(!CheckAttackable()) CancelAttack();
        }
        public override void OnExit()
        {
            controller.animator.speed = 1.0f;//�����Ă�Ƃ��Ɏ��񂾎��p
            cts?.Dispose();
            controller.animator.SetBool(controller.MonsterAnimPar.Attack_Hash, false);
        }

        protected virtual async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        {
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName))
            {
                controller.animator.Play(controller.MonsterAnimPar.attackAnimClipName);
            }

            //����continuous�̃����X�^�[����Ȃ����ǂł����s�[�g�U���̎�(�I�[�N�Ƃ��h���S��)
            if(controller is IRepeatAttack repeat) attackArguments.repeatCount = repeat.repeatCount;
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
                attackArguments.attackEffectAction?.Invoke();
                if(attackArguments.repeatCount == 0)
                {
                    var currentTargets = attackArguments.getTargets();
                    if (target == null) return;
                    if (!currentTargets.Contains(target)) currentTargets.Add(target);
                    currentTargets.ForEach(target =>
                    {
                        AddDamageToTarget(target);
                        if (attackArguments.specialEffectAttack != null) attackArguments.specialEffectAttack?.Invoke(target);
                    });
                }
                else
                {
                    var remaining = (int)attackArguments.repeatCount;
                    var repeatInterval = GetRepeatInterval(startNormalizeTime,remaining);
                   
                    attackArguments.actionByLeftAttackLength?.Invoke(GetCurrentNormalizedTime);
                    Debug.Log($"{GetCurrentNormalizedTime()},���s�[�g�U���J�n���܂�");

                    while(remaining > 0 && GetCurrentNormalizedTime() < 1.0f && !cts.IsCancellationRequested
                        && !isInterval)
                    {
                        repeatInterval = GetRepeatInterval(startNormalizeTime, remaining);
                        Debug.Log($"���s�[�g{GetCurrentNormalizedTime()}");
                        if (target == null) break;
                        var currentTargets = attackArguments.getTargets();
                        if (!currentTargets.Contains(target)) currentTargets.Add(target);
                        currentTargets.ForEach(target =>
                        {
                            AddDamageToTarget(target);
                            if (attackArguments.specialEffectAttack != null) attackArguments.specialEffectAttack?.Invoke(target);
                        });
                        remaining--;
                        await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval), cancellationToken: cts.Token);
                    }
                }                  
            }
            catch (OperationCanceledException)
            {
                var elapsedTime = (now - startNormalizeTime) * clipLength;
                leftLengthTime = Mathf.Max(0f, clipLength - elapsedTime / stateAnimSpeed);
                isAttacking = false;
            }
            catch (ObjectDisposedException) { }
            finally { if(attackArguments.attackEndAction != null) attackArguments.attackEndAction?.Invoke();}
            leftLengthTime = 0f;
        }
        protected virtual async UniTask Attack_Long(LongAttackArguments longAttackArguments)
        {
            if (controller is IRepeatAttack repeat) longAttackArguments.repeatCount = repeat.repeatCount;
            float now = 0f;
            LongDistanceAttack<T> nextMover = null;
            try
            {
                if (!controller.statusCondition.Freeze.isActive) LookToTarget();
                controller.animator.speed = 1.0f;
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName)
                , cancellationToken: cts.Token);
                Debug.Log(target.gameObject.name);
                startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - startNormalizeTime >= attackEndNomTime;
                });

                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);
                longAttackArguments.attackEffectAction?.Invoke();
                if (longAttackArguments.repeatCount == 0)
                {
                    nextMover = longAttackArguments.getNextMover();
                    if (nextMover == null) return;
                    longAttackArguments.moveAction?.Invoke(nextMover);
                }
                else
                {
                    var remaining = longAttackArguments.repeatCount;
                    var repeatInterval = GetRepeatInterval(startNormalizeTime, remaining);
                    while (remaining > 0 && GetCurrentNormalizedTime() < 1.0f && !cts.IsCancellationRequested
                        && !isInterval)
                    {
                        LookToTarget();
                        repeatInterval = GetRepeatInterval(startNormalizeTime, remaining);
                        Debug.Log("�����O�Ń��s�[�g�ȃA�^�b�N�I�I");
                        nextMover = longAttackArguments.getNextMover();
                        if (nextMover == null) return;
                        longAttackArguments.moveAction?.Invoke(nextMover);
                        remaining--;
                        await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval), cancellationToken: cts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                var elaspedTime = (now - startNormalizeTime) * clipLength;
                leftLengthTime = Mathf.Max(0f, clipLength - elaspedTime) / stateAnimSpeed;
                isAttacking = false;
            }
            catch (ObjectDisposedException) { }
            finally { longAttackArguments.attackEndAction?.Invoke(); }
            leftLengthTime = 0f;
        }
        protected virtual void AddDamageToTarget(UnitBase currentTarget)
        {
            if (currentTarget.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                Debug.Log($"{controller.gameObject.name}�̃A�^�b�N");
                unitDamagable.Damage(attackAmount);
                EffectManager.Instance.hitEffect.GenerateHitEffect(currentTarget);
            }
        }
        protected virtual bool CheckAttackable()
        {
            Debug.Log(target.gameObject.name);
            var canAttack = false;
            var targetPos = Vector3.zero;
            var isDead = target.isDead;
            var isTransparent = target.statusCondition.Transparent.isActive;
            var collider = target.GetComponent<Collider>();
            var closestPos = collider.ClosestPoint(controller.transform.position);
            var myMoveType = controller.moveType;
            var effectiveMoveSide = myMoveType switch
            {
                MoveType.Walk => MoveType.Walk,
                MoveType.Fly => MoveType.Fly | MoveType.Walk,
                _ => default
            };
            var targetMoveType = target.moveType;
            targetPos = PositionGetter.GetFlatPos(closestPos);
            var myPos = PositionGetter.GetFlatPos(controller.transform.position);
            var isConfused = controller.statusCondition.Confusion.isActive;
            var targetSide = target.GetUnitSide(controller.ownerID);
            var effectiveSide = isConfused switch
            {
                true => Side.PlayerSide | Side.EnemySide,
                false => Side.EnemySide,
            };

            canAttack = (targetPos - myPos).magnitude <= attackRange && !isDead
                && (targetSide & effectiveSide) != 0 && !isTransparent && (effectiveMoveSide & targetMoveType) != 0;// && !isDead;         
            Debug.Log($"[�����`�F�b�N] ����: {canAttack}, �˒�: {controller.MonsterStatus.AttackRange}");
            return canAttack;
        }      
        protected float GetAttackRange()
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
            Debug.Log("�L�����̕����������܂�");
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
            Debug.Log("�G���͈͊O�ɍs���܂���");
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
                    var isTransparent = cmp.statusCondition.Transparent.isActive;
                    var isNonTarget = cmp.statusCondition.NonTarget.isActive;
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
                          && (moveType & effectiveMoveSide) != 0 && isSummoned && !isTransparent && !isNonTarget;
                    }
                    //�����^���[������!isTransparent && !isNonTarget����Ȃ����Ǐ��������������炻��������Ԉُ��
                    //�^���[�ɕt�^�������o�Ă��邩������Ȃ�����ꉞ
                    return (enemySide & effectiveSide) != 0 && !isDead
                            && (moveType & effectiveMoveSide) != 0 && !isTransparent && !isNonTarget;
                }).ToArray(),

                MonsterAttackType.OnlyBuilding => sortedArray.Where(cmp =>
                {
                    var isTransparent = cmp.statusCondition.Transparent.isActive;
                    var isNonTarget = cmp.statusCondition.NonTarget.isActive;
                    var enemySide = cmp.GetUnitSide(controller.ownerID);
                    var isDead = cmp.isDead;
                    var building = cmp is IBuilding;
                    return enemySide != Side.PlayerSide && !isDead
                          && building && !isTransparent && !isNonTarget;
                }).ToArray(),

                MonsterAttackType.GroundedAndEveryThing => sortedArray.Where(cmp =>
                {
                    var enemySide = cmp.GetUnitSide(controller.ownerID);
                    var isTransparent = cmp.statusCondition.Transparent.isActive;
                    var isNonTarget = cmp.statusCondition.NonTarget.isActive;
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
                               && isSummoned && !isTransparent && !isNonTarget;
                    }
                    //��̂�Ɠ������R
                    return (enemySide & effectiveSide) != 0 && !isDead && !isTransparent && !isNonTarget;
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
                var argments = new SimpleAttackArguments
                {
                    getTargets = () => target != null ? new List<UnitBase> { target } : Enumerable.Empty<UnitBase>().ToList()
                };
                Attack_Generic(argments).Forget();
            }
            else if (controller.MonsterStatus.AttackType == AttackType.Range)
            {
                var argments = new SimpleAttackArguments
                {
                    getTargets = () => controller.GetUnitInWeponRange().Invoke()
                };
                Attack_Generic(argments).Forget();
            }
            else if (controller.MonsterStatus.AttackType == AttackType.Long)
            {
                var arguments = new LongAttackArguments();
                Attack_Long(arguments).Forget();
            }
        }
        public virtual async void StopAnimFromEvent()
        {
            Debug.Log("�C�x���g���Ă΂�܂���");
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
        protected float GetCurrentNormalizedTime()
        {
            var current = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            Debug.Log($"���݂�{current},�X�^�[�g��{startNormalizeTime}");
            return current - startNormalizeTime;
        }
        protected float GetRepeatInterval(float startNormalizedTime,int repeatCount)
        {
            var now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            var elapsedNormalizedTime = now - startNormalizeTime;
            var startLength = elapsedNormalizedTime * clipLength;
            var leftLength = clipLength - startLength;
            var repeatInterval = leftLength /  (float)repeatCount;
            return (repeatInterval / stateAnimSpeed) / controller.animator.speed;
        }     
    }
}

