using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Game.Monsters;
using Game.Monsters.KingDragon;
using System.Threading;
using Unity.VisualScripting;
using static Game.Monsters.KingDragon.AttackState;
using System.Linq;
using UnityEngine.UIElements;

namespace Game.Monsters.KingDragon
{
    public class KingDragonMethod
    {
        public KingDragonMethod(KingDragonController controller)
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
            controller.animator.SetBool(kingDragonAnimPar.Attack2_Hash,false);
            controller.animator.SetBool(kingDragonAnimPar.Attack_Hash,true);
            var animationInfo = _attackState.animationInfo;
            Debug.Log("攻撃開始します");
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(kingDragonAnimPar.attackAnimClipName))
            {
                controller.animator.Play(kingDragonAnimPar.attackAnimClipName);
            }
            var cts = controller.AttackState.cts;
            float now = 0f;
            try
            {
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
                _attackState.canChangeAttackType = true;
                if (attackArguments.specialEffectAttack != null) attackArguments.specialEffectAttack?.Invoke(target);
                await UniTask.WaitUntil(() => controller.animator.GetCurrentNormalizedTime(_attackState.startNormalizeTime) >= 1.0f
                                        ,cancellationToken:cts.Token);
            }
            catch (OperationCanceledException)
            {
                now = controller.animator.GetCurrentNormalizedTime(_attackState.startNormalizeTime);
                var elapsedTime = (now - _attackState.startNormalizeTime) * animationInfo.simpleAttackClipLength;
                _attackState.leftLengthTime = Mathf.Max(0f, animationInfo.simpleAttackClipLength - elapsedTime /
                                                       animationInfo.simpleAttackAnimSpeed);
                //_attackState.isAttacking = false;
            }
            catch (ObjectDisposedException) { return; }
            finally { if (attackArguments.attackEndAction != null) attackArguments.attackEndAction?.Invoke(); }
            if(!cts.IsCancellationRequested) _attackState.leftLengthTime = 0f;
        }
        public async UniTask Attack_Long(LongAttackArguments<KingDragonController> longAttackArguments)
        {
            _attackState.canChangeAttackType = true;
            var animationInfo = _attackState.animationInfo;
            controller.animator.SetBool(kingDragonAnimPar.Attack_Hash, false);
            controller.animator.SetBool(kingDragonAnimPar.Attack2_Hash, true);
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(kingDragonAnimPar.attack_2AnimClipName))
            {
                controller.animator.Play(kingDragonAnimPar.attack_2AnimClipName);
            }

            float now = 0f;
            LongDistanceAttack<KingDragonController> nextMover = null;
            var cts = controller.AttackState.cts;
            try
            {
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
                _attackState.isShotingFire = true;
                longAttackArguments.attackEffectAction?.Invoke();
                var remaining = longAttackArguments.repeatCount;
                var repeatInterval = controller.animator.GetRepeatInterval(_attackState.startNormalizeTime, remaining
                                                               ,animationInfo.longAttackClipLength
                                                               ,animationInfo.longAttackAnimSpeed);
                ;

                while (remaining > 0 && controller.animator.GetCurrentNormalizedTime(_attackState.startNormalizeTime) < 1.0f
                    && !cts.IsCancellationRequested&& !_attackState.isInterval)
                {
                    repeatInterval = controller.animator.GetRepeatInterval(_attackState.startNormalizeTime, remaining
                                                               ,animationInfo.longAttackClipLength
                                                               ,animationInfo.longAttackAnimSpeed);
                    Debug.Log("ロングでリピートなアタック！！");
                    nextMover = longAttackArguments.getNextMover();
                    if (nextMover == null) return;
                    longAttackArguments.moveAction?.Invoke(nextMover);
                    if (_attackState.targetEnemy == null) cts?.Cancel();
                    LookToTarget();
                    remaining--;
                    await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval), cancellationToken: cts.Token);
                }

            }
            catch (OperationCanceledException)
            {
                var elaspedTime = (now - _attackState.startNormalizeTime) * animationInfo.longAttackClipLength;
                _attackState.leftLengthTime = Mathf.Max(0f,animationInfo.longAttackClipLength - elaspedTime)
                                                        / animationInfo.longAttackAnimSpeed;
            }
            catch (ObjectDisposedException) { return; }
            finally 
            {
                _attackState.isShotingFire = false;
                longAttackArguments.attackEndAction?.Invoke();
            }
            if (!cts.IsCancellationRequested) _attackState.leftLengthTime = 0f;
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
            var attackAmount = controller.KingDragonStatus.AttackAmount;
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
            _attackState.isChecking = true;
            _attackState.targetEnemy = null;
            _attackState.isAttacking = false;
            _attackState.isWaitingLeftTime = true;
            controller.animator.speed = 1.0f;
            controller.animator.Play("Idle");
            controller.animator.SetBool(kingDragonAnimPar.Attack_Hash, false);
            controller.animator.SetBool(kingDragonAnimPar.Attack2_Hash, false);
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_attackState.leftLengthTime)
                                    , cancellationToken: controller.GetCancellationTokenOnDestroy());
                Debug.Log($"攻撃続行{ContinueCurrentAttackProcess(out var ex)},{ex}");
                if (ContinueCurrentAttackProcess(out var newTarget))
                {
                    if (_attackState.canChangeAttackType) _attackState.ChangeCurrentAttackType(_attackState.currentAttackType);
                    _attackState.isContineAttack = true;
                    var continueAttackInterval = _attackState.animationInfo.interval - _attackState.leftLengthTime;
                    await UniTask.Delay(TimeSpan.FromSeconds(continueAttackInterval)
                                        ,cancellationToken: controller.GetCancellationTokenOnDestroy());
                    Debug.Log($"{_attackState.leftLengthTime},{continueAttackInterval}");
                    _attackState.cts = new CancellationTokenSource();
                    _attackState.targetEnemy = newTarget;
                    _attackState.isAttacking = true;
                    _attackState.Attack();
                    return;
                }
            }
            catch (OperationCanceledException){ return;}
            finally
            {
                _attackState.leftLengthTime = 0f;
                _attackState.isWaitingLeftTime = false;
                _attackState.isContineAttack = false;
                _attackState.isChecking = false;
            }
            controller.ChangeState( _attackState._nextState);
        }

        public bool ContinueCurrentAttackProcess(out UnitBase newTarget)
        {
           
            var nextAttackType = _attackState.currentAttackType == CurrentAttackType.Simple && _attackState.canChangeAttackType
                                  ? CurrentAttackType.Long 
                                  : _attackState.currentAttackType == CurrentAttackType.Long && _attackState.canChangeAttackType
                                  ? CurrentAttackType.Simple
                                  : _attackState.currentAttackType;

            var target = SetTarget(nextAttackType);
            if(target == null && nextAttackType == CurrentAttackType.Long)
            {
                newTarget = null;
                return false;
            }
            else if(target == null && nextAttackType == CurrentAttackType.Simple)
            {
                target = SetTarget(CurrentAttackType.Long);
                if (target == null)
                {
                    newTarget = null;
                    return false;
                }
                else _attackState.ChangeCurrentAttackType(nextAttackType);
            }           
            newTarget = target;
            return true;
        }

        public UnitBase SetTarget(CurrentAttackType targetAttackType)
        {
            var targetRadius = targetAttackType switch
            {
                CurrentAttackType.Simple => controller.KingDragonStatus.AttackSimpleRange,
                CurrentAttackType.Long => controller.KingDragonStatus.AttackLongRange,
                _ => default
            };

            var effectiveMoveType = targetAttackType switch
            {
                CurrentAttackType.Simple => MoveType.Walk,
                CurrentAttackType.Long => MoveType.Walk | MoveType.Fly,
                _ => default
            };
            var sortedList = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>
                (controller.gameObject, targetRadius).ToList();

            var filterdList = sortedList.Where(unit =>
            {
                if (unit is ISummonbable summonbable && !summonbable.isSummoned) return false;
                if (unit is IInvincible invincible && invincible.IsInvincible) return false;
                var isDead = unit.isDead;
                var side = unit.GetUnitSide(controller.ownerID);
                var isTransparent = unit.statusCondition.Transparent.isActive;
                var isNonTarget = unit.statusCondition.NonTarget.isActive;
                var moveType = unit.moveType;
                if (isDead || side == Side.PlayerSide || isTransparent || isNonTarget
                   || (moveType & effectiveMoveType) == 0) return false;
                return true;
            }).ToList();
            if (filterdList.Count == 0) return null;
            else return filterdList[0];
        }
    }
}

