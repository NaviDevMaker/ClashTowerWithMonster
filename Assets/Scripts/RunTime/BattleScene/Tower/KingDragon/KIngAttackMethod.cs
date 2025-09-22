using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Game.Monsters;
using Game.Monsters.KingDragon;
using System.Threading;
using Unity.VisualScripting;
using static Game.Monsters.KingDragon.AttackState;
using System.Linq;

namespace Game.Monsters.KingDragon
{
    public class KingAttackMethod
    {
        public KingAttackMethod(KingDragonController controller)
        {
            this.controller = controller;
            _attackState = controller.AttackState;
            kingDragonAnimPar = controller.KingDragonAnimPar;
        }
        KingDragonController controller;
        AttackState _attackState;
        KingDragonAnimPar kingDragonAnimPar;
        public async UniTask Attack_Simple(SimpleAttackArguments attackArguments)
        {
            var animationInfo = _attackState.animationInfo;
            Debug.Log("攻撃開始します");
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(kingDragonAnimPar.attackAnimClipName))
            {
                controller.animator.SetBool(kingDragonAnimPar.Attack_Hash, true);
                controller.animator.Play(kingDragonAnimPar.attackAnimClipName);
            }

            float now = 0f;
            try
            {
                var cts = controller.AttackState.cts;
                LookToTarget();
                await UniTask.WaitUntil(() =>
                {
                    if (controller.isDead) return false;
                    return controller.animator.GetCurrentAnimatorStateInfo(0).IsName(kingDragonAnimPar.attackAnimClipName);
                }, cancellationToken: cts.Token);
                controller.animator.speed = 1.0f;
                _attackState.startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    if (controller.isDead) return true;
                    now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - _attackState.startNormalizeTime >= animationInfo.simpleAttackEndNomTime;
                });
                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);
                attackArguments.attackEffectAction?.Invoke();
                var target = _attackState.targetEnemy;
                if (target == null) return;
                AddDamageToTarget(target);
                if (attackArguments.specialEffectAttack != null) attackArguments.specialEffectAttack?.Invoke(target);
            }
            catch (OperationCanceledException)
            {
                var elapsedTime = (now - _attackState.startNormalizeTime) * animationInfo.simpleAttackClipLength;
                _attackState.leftLengthTime = Mathf.Max(0f, animationInfo.simpleAttackClipLength - elapsedTime /
                                                       animationInfo.simpleAttackAnimSpeed);
                _attackState.isAttacking = false;
            }
            catch (ObjectDisposedException) { }
            finally { if (attackArguments.attackEndAction != null) attackArguments.attackEndAction?.Invoke(); }
            _attackState.leftLengthTime = 0f;
        }
        public async UniTask Attack_Long(LongAttackArguments<KingDragonController> longAttackArguments)
        {
            var animationInfo = _attackState.animationInfo;
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(kingDragonAnimPar.attack_2AnimClipName))
            {
                controller.animator.SetBool(kingDragonAnimPar.Attack2_Hash, true);
                controller.animator.Play(kingDragonAnimPar.attack_2AnimClipName);
            }

            float now = 0f;
            LongDistanceAttack<KingDragonController> nextMover = null;
            try
            {
                var cts = controller.AttackState.cts;
                if (!controller.statusCondition.Freeze.isActive) LookToTarget();
                controller.animator.speed = 1.0f;
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName(kingDragonAnimPar.attack_2AnimClipName)
                                              , cancellationToken: cts.Token);
                _attackState.startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - _attackState.startNormalizeTime >= animationInfo.longAttackEndNomTime;
                });

                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);
                longAttackArguments.attackEffectAction?.Invoke();
                var remaining = longAttackArguments.repeatCount;
                var repeatInterval = controller.animator.GetRepeatInterval(_attackState.startNormalizeTime, remaining
                                                               ,animationInfo.longAttackClipLength,animationInfo.longAttackAnimSpeed);
                ;
                while (remaining > 0 && controller.animator.GetCurrentNormalizedTime(_attackState.startNormalizeTime) < 1.0f
                    && !cts.IsCancellationRequested&& !_attackState.isInterval)
                {
                    LookToTarget();
                    repeatInterval = controller.animator.GetRepeatInterval(_attackState.startNormalizeTime, remaining
                                                               ,animationInfo.longAttackClipLength, animationInfo.longAttackAnimSpeed);
                    Debug.Log("ロングでリピートなアタック！！");
                    nextMover = longAttackArguments.getNextMover();
                    if (nextMover == null) return;
                    longAttackArguments.moveAction?.Invoke(nextMover);
                    remaining--;
                    await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval), cancellationToken: cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                var elaspedTime = (now - _attackState.startNormalizeTime) * animationInfo.longAttackClipLength;
                _attackState.leftLengthTime = Mathf.Max(0f,animationInfo.longAttackClipLength - elaspedTime)
                                                        / animationInfo.longAttackAnimSpeed;
                _attackState.isAttacking = false;
            }
            catch (ObjectDisposedException) { }
            finally { longAttackArguments.attackEndAction?.Invoke(); }
            _attackState.leftLengthTime = 0f;
        }
        public void LookToTarget()
        {
            var target = _attackState.targetEnemy;
            Debug.Log("キャラの方向を向きます");
            if (target == null) return;
            Renderer renderer = target.BodyMesh;
            if (renderer == null) return;
            var lookPos = renderer.bounds.center;
            var myCriterionPos = controller.BodyMesh.bounds.center;
            var direction = lookPos - myCriterionPos;
            direction.y = 0f;
            var rotation = Quaternion.LookRotation(direction);
            controller.transform.rotation = rotation;
        }

        void AddDamageToTarget(UnitBase currentTarget)
        {
            var attackAmount = controller.TowerStatus.AttackAmount;
            if (currentTarget.TryGetComponent<IUnitDamagable>(out var unitDamagable))
            {
                Debug.Log($"{controller.gameObject.name}のアタック");
                unitDamagable.Damage(attackAmount);
                EffectManager.Instance.hitEffect.GenerateHitEffect(currentTarget);
            }
        }

        public async void CancelAttack()
        {
            Debug.Log("敵が範囲外に行きました");
            _attackState.cts?.Cancel();
            _attackState.isAttacking = false;
            _attackState.isWaitingLeftTime = true;
            controller.animator.speed = 1.0f;
            controller.animator.Play("Idle");
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_attackState.leftLengthTime)
                                    , cancellationToken: controller.GetCancellationTokenOnDestroy());
                if (ContinueAttackState())
                {
                    _attackState.isContineAttack = true;
                    var continueAttackInterval = _attackState.animationInfo.interval - _attackState.leftLengthTime;
                    await UniTask.Delay(TimeSpan.FromSeconds(continueAttackInterval)
                                        , cancellationToken: controller.GetCancellationTokenOnDestroy());
                    controller.animator.Play(kingDragonAnimPar.attackAnimClipName);
                    _attackState.cts = new CancellationTokenSource();
                    _attackState.isAttacking = true;
                    _attackState.Attack();
                    return;
                }
            }
            catch (OperationCanceledException){ return;}
            finally
            {
                _attackState.isWaitingLeftTime = false;
                _attackState.isContineAttack = false;
            }
            controller.ChangeState( _attackState._nextState);
        }

        bool ContinueAttackState()
        {
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject
                                                                       , controller.TowerStatus.SearchRadius);
            var nextAttackType = _attackState.currentAttackType == CurrentAttackType.Simple ? CurrentAttackType.Long
                                  : CurrentAttackType.Simple;
            var effectiveMoveType = nextAttackType switch
            {
                CurrentAttackType.Simple => MoveType.Walk,
                CurrentAttackType.Long => MoveType.Walk | MoveType.Fly,
                _ => default
            };

            var filteredArray = sortedArray.Where(unit =>
            {
                if (unit.TryGetComponent<ISummonbable>(out var summonbable) && !summonbable.isSummoned) return false;
                var enemySide = unit.GetUnitSide(controller.ownerID);
                var isDead = unit.isDead;
                var isTransparent = unit.statusCondition.Transparent.isActive;
                var isNonTarget = unit.statusCondition.NonTarget.isActive;
                var moveType = unit.moveType;
                //ここタワーだから!isTransparent && !isNonTargetいらないけど将来もしかしたらそういう状態異常を
                //タワーに付与するやつが出てくるかもしれないから一応
                return (enemySide & Side.EnemySide) != 0 && !isDead
                        && (moveType & effectiveMoveType) != 0 && !isTransparent && !isNonTarget;
            }).ToArray();

            if (filteredArray.Length == 0) return false;
            var newTarget = filteredArray[0];
            _attackState.targetEnemy = newTarget;
            return true;
        }
    }
}

