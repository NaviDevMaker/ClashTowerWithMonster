using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;
using Game.Monsters.Slime;
using System.Threading;
using DG.Tweening;
using Mono.Cecil;
using static UnityEngine.GraphicsBuffer;

namespace Game.Monsters
{
    public class ChaseStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public ChaseStateBase(T controller) : base(controller) { }

        GameObject targetTower = null;
        GameObject targetEnemy = null;

        bool reachTargetEnemy = false;
        bool isChasing = false;
        protected float flyingOffsetY = 0f;
        CancellationTokenSource cts = new CancellationTokenSource();
        SemaphoreSlim moveSemaphoreSlim = new SemaphoreSlim(1, 1);
        MonsterAttackType myMonsterType;
        public override void OnEnter()
        {
            myMonsterType = controller.MonsterStatus.MonsterAttackType;
            cts = new CancellationTokenSource();
            controller.animator.SetBool(controller.MonsterAnimPar.Chase, true);
            nextState = controller.AttackState;
            SetTargetTower();
            ChaseTarget().Forget();
        }
        public override void OnUpdate()
        {
            Debug.Log(isChasing);
            if(controller.isKnockBacked)
            {
                try
                {
                    cts?.Cancel();
                    controller.ChangeState(this);
                    Debug.Log("�m�b�N�o�b�N���ꂽ��");
                }
                catch (ObjectDisposedException)
                {

                }

                return;
            }

            Debug.Log(targetEnemy);
            if (reachTargetEnemy)
            {
                controller.ChangeState(nextState);
                return;
            }
            if (myMonsterType == MonsterAttackType.ToEveryThing) EvaluateNewTargetAndChase();
        }
        public override void OnExit()
        {
            reachTargetEnemy = false;
            targetEnemy = null;
            targetTower = null;
            controller.animator.SetBool(controller.MonsterAnimPar.Chase, false);
            //cts?.Dispose();
        }
        protected virtual void SetTargetTower()
        {
            Debug.Log("�^�[�Q�b�g�̃^���[���擾���܂�");
            TowerControlller[] targetTowers = GameObject.FindObjectsByType<TowerControlller>(sortMode: FindObjectsSortMode.None);

            List<TowerControlller> toList = new List<TowerControlller>(targetTowers);
            toList = toList
                .Where(tower =>
                { 
                    var isDead = tower.isDead;
                    var side = tower.Side;
                    return !isDead && side != controller.Side;
                }) 
                .OrderBy(tower => Vector3.Distance(controller.transform.position, tower.transform.position)).ToList();
            if (toList.Count > 0) targetTower = toList[0].gameObject;          
            //foreach (var tower in toList)
            //{
            //    if (tower.Side == controller.Side) continue;
            //    else targetTower = tower.gameObject; break;
            //}
        }

        async UniTask ChaseTarget()
        {
            if (targetTower == null)
            {
                Debug.LogError("�Q�[���Z�b�g�ł�");
                return;
            }
                if (isChasing || controller.isKnockBacked) return;
            isChasing = true;
            try
            {
                await moveSemaphoreSlim.WaitAsync();
                Debug.Log("�ǐՂ��܂�");
                cts = new CancellationTokenSource();

                //�{�Ԃ�return�͂��āA�Q�[�����I���܂ł͂���
                if (targetTower == null)
                {
                    Debug.LogWarning("ChaseTarget���~: targetTower��null");
                    //return;
                }
                GameObject target = default;
                if (targetEnemy == null) target = targetTower;
                else target = targetEnemy;
                
                if(target != null) Debug.Log($"�ǐՑΏ�: {target.name}");
                var moveStep = controller.MonsterStatus.MoveStep;
                var perPixelMoveTime = (1 / moveStep) / controller.MonsterStatus.MoveSpeed;
                var offset = Vector3.zero;
                var targetPos = Vector3.zero;
                //var myselfPos = controller.transform.position + Vector3.up * flyingOffsetY;
                Tween moveTween = null;
                UniTask moveTask = default;
                if (target == targetEnemy)
                {

                    Debug.Log("�G��ǐՒ�");
                    //targetPos = target.transform.position;

                    targetPos = target.transform.position;
                    targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;

                    while (Vector3.Distance(controller.transform.position, targetPos) > controller.MonsterStatus.AttackRange
                        && target != null)
                    {
                        targetPos = target.transform.position;
                        targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                        Debug.Log($"����{controller.transform.position}����{targetPos}");
                        var direction = (targetPos - controller.transform.position).normalized;
                        var perTargetPos = GetPerTargetPos(controller.transform.position, direction);
                        var perDirection = perTargetPos - controller.transform.position;
                        var targetRot = Quaternion.LookRotation(perDirection);
                        controller.transform.rotation = targetRot;

                        moveTween = controller.transform.DOMove(perTargetPos, perPixelMoveTime);
                        moveTask = moveTween.ToUniTask(cancellationToken: cts.Token);

                        while (!moveTask.Status.IsCompleted() && !cts.IsCancellationRequested)
                        {
                            var isDead = controller.isDead;
                            if (isDead) { cts?.Cancel();  break; }
                            if (Vector3.Distance(controller.transform.position, targetPos) <= controller.MonsterStatus.AttackRange
                              || targetEnemy == null)
                            {
                                Debug.Log("�G�ɓ��� || �^�[�Q�b�g���͈͊O�ɂ����܂���");

                                cts?.Cancel();
                                SetAttackStateField(target);
                                break;
                            }

                            await UniTask.Yield();
                        }

                        if (cts.IsCancellationRequested)
                        {
                            moveTween.Kill();
                            break;
                        }
                        await moveTask;
                    }
                   
                }
                else if (target == targetTower)
                {
                    var targetCollider = target.GetComponent<Collider>();
                  
                    Debug.Log("�^���[��ǐՒ�");

                    if (target == null || targetCollider == null) return;
                    targetPos = targetCollider.ClosestPoint(controller.transform.position);
                    targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                    Debug.Log(targetPos);
                      
                        while (Vector3.Distance(controller.transform.position, targetPos) > controller.MonsterStatus.AttackRange
                         && target != null)
                        {

                            targetPos = targetCollider.ClosestPoint(controller.transform.position);
                            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                            var direction = (targetPos - controller.transform.position).normalized;
                            Debug.Log(target.gameObject.name);
                            var perTargetPos = GetPerTargetPos(controller.transform.position, direction);

                            var perDirection = perTargetPos - controller.transform.position;
                            var targetRot = Quaternion.LookRotation(perDirection);
                            controller.transform.rotation = targetRot;

                            moveTween = controller.transform.DOMove(perTargetPos, perPixelMoveTime);
                            moveTask = moveTween.ToUniTask(cancellationToken: cts.Token);//������̓���

                            while (!moveTask.Status.IsCompleted() && !cts.IsCancellationRequested)
                            {
                                var isDead = controller.isDead;
                            �@�@�@
                                if (isDead) { cts?.Cancel(); break; }

                                if (Vector3.Distance(controller.transform.position, targetPos) <= controller.MonsterStatus.AttackRange)
                                {
                                    Debug.Log("�G�ɓ���");
                                    cts?.Cancel();
                                    SetAttackStateField(target, targetPos);
                                    break;
                                }
                                await UniTask.Yield();
                            }
                            //�L�����Z�����Ă����Task��await���Ă��Ȃ����炱���܂Ői�݁A����await moveTask�ɓ��B����ƃG���[���o�Ă��܂��̂ł�����break
                            if (cts.IsCancellationRequested)
                            {
                                Debug.Log("�m�b�N�o�b�N����܂���");
                                moveTween.Kill();
                                break;
                            }
                            //��{�I�ɂ͂����܂ŃL�����Z�����ꂸ���B����
                            await moveTask;
                        }
                }
            }
            finally
            {
                Debug.Log("��������������������������������");
                isChasing = false;
                moveSemaphoreSlim.Release();
                cts?.Dispose();
            }
        }

        void SetAttackStateField(GameObject target, Vector3 targetPos = default)
        {
            reachTargetEnemy = true;

            AttackStateBase<T> attackState = controller.AttackState;
            var unitBase = target.GetComponent<UnitBase>();

            attackState.target = unitBase;
            attackState.flyingOffsetY = flyingOffsetY;
        }
        Vector3 GetPerTargetPos(Vector3 currentPos, Vector3 direction)
        {
            var targetPos = currentPos + direction;
            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
            return targetPos;
        }

        void EvaluateNewTargetAndChase()
        {
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject, controller.MonsterStatus.ChaseRange);

            var filterdArray = sortedArray.Where(cmp =>
            {
                var enemyType = cmp.Side;
                var isDead = cmp.isDead;
                return enemyType != controller.Side && !isDead;// 
            }).ToArray();

            if (filterdArray.Length == 0)
            {
                targetEnemy = null;
                if(targetTower != null && !isChasing) ChaseTarget().Forget();
            }
            else
            {
                var newTarget = filterdArray[0].gameObject;
                //�����G�Ȃ牽�����Ȃ�
                if ((targetEnemy == newTarget || targetTower == newTarget)
                    && isChasing) return;
                Debug.Log("�^�[�Q�b�g�ύX");
                targetEnemy = newTarget;

                //Dispose()���ꂽ�̂����Ȃ��悤�ɂ���������킩��Ȃ������A����
                try
                {
                    cts?.Cancel();
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.LogWarning("���ł�Dispose����Ă邽�߁ACancel�̓X�L�b�v����܂���: " + ex.Message);
                }
                ChaseTarget().Forget();
            }
        }
    }
}

