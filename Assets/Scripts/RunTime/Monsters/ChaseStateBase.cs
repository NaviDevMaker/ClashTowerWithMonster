using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;

namespace Game.Monsters
{
    public class ChaseStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public ChaseStateBase(T controller) : base(controller) { }

        GameObject targetTower = null;
        GameObject targetEnemy = null;

        bool reachTargetEnemy = false;
        bool isChasing = false;
        int moveSpeed = 0;
        protected float flyingOffsetY = 0f;
        public CancellationTokenSource cts = new CancellationTokenSource();
        SemaphoreSlim moveSemaphoreSlim = new SemaphoreSlim(1, 1);
        MonsterAttackType myMonsterAttackType;
        public override void OnEnter()
        {
            moveSpeed = controller.BuffStatus(BuffType.Speed, (int)controller.MonsterStatus.MoveSpeed);
            myMonsterAttackType = controller.MonsterStatus.MonsterAttackType;
            cts = new CancellationTokenSource();
            controller.animator.SetBool(controller.MonsterAnimPar.Chase, true);
            nextState = controller.AttackState;
            SetTargetTower();
            EvaluateNewTargetAndChase();
            ChaseTarget().Forget();
        }
        public override void OnUpdate()
        {
            moveSpeed = controller.BuffStatus(BuffType.Speed, (int)controller.MonsterStatus.MoveSpeed);
            var isBuffed = controller.statusCondition.BuffSpeed.isActive;
            if (isBuffed) { var newSpeed = 1.3f; controller.animator.speed = newSpeed; }
            Debug.Log(isChasing);
            if(controller.isKnockBacked_Unit)
            {
                try
                {
                    cts?.Cancel();
                    controller.ChangeState(this);
                    Debug.Log("ノックバックされたよ");
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
           EvaluateNewTargetAndChase();// if (myMonsterAttackType == MonsterAttackType.ToEveryThing) 
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
            Debug.Log("ターゲットのタワーを取得します");
            TowerControlller[] targetTowers = GameObject.FindObjectsByType<TowerControlller>(sortMode: FindObjectsSortMode.None);

            List<TowerControlller> toList = new List<TowerControlller>(targetTowers);
            toList = toList
                .Where(tower =>
                { 
                    var isDead = tower.isDead;
                    var side = tower.GetUnitSide(controller.ownerID);
                    return !isDead && side == Side.EnemySide;
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
                Debug.LogWarning("ゲームセットです");
                return;
            }
            if (isChasing || controller.isKnockBacked_Spell) return;
            isChasing = true;
            try
            {
                await moveSemaphoreSlim.WaitAsync();
                Debug.Log("追跡します");
                cts = new CancellationTokenSource();

                //本番はreturnはつけて、ゲームが終わるまではかｋ
                if (targetTower == null)
                {
                    Debug.LogWarning("ChaseTarget中止: targetTowerがnull");
                    //return;
                }
                GameObject target = default;
                if (targetEnemy == null) target = targetTower;
                else target = targetEnemy;
                
                if(target != null) Debug.Log($"追跡対象: {target.name}");
                var moveStep = controller.MonsterStatus.MoveStep;
                var offset = Vector3.zero;
                var targetPos = Vector3.zero;
                //var myselfPos = controller.transform.position + Vector3.up * flyingOffsetY;
                Tween moveTween = null;
                UniTask moveTask = default;

                var targetCollider = target.GetComponent<Collider>();
                targetPos = targetCollider.ClosestPoint(controller.transform.position);//target.transform.position;
                targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                var flatTargetPosition = PositionGetter.GetFlatPos(targetPos);
                var flatMyPosition = PositionGetter.GetFlatPos(controller.transform.position);
                if (Vector3.Distance(flatMyPosition, flatTargetPosition) <= controller.MonsterStatus.AttackRange)
                {
                    Debug.Log("ステートに入った瞬間に範囲内だったため、即攻撃へ");
                    SetAttackStateField(target, targetPos);
                    return;
                }
                if (target == targetEnemy)
                {
                    Debug.Log("敵を追跡中");
                    //targetPos = target.transform.position;


                    //var myRadius = controller.GetComponent<Collider>().bounds.extents.magnitude;
                    while (Vector3.Distance(flatMyPosition, flatTargetPosition) > controller.MonsterStatus.AttackRange
                        && target != null)
                    {
                        var perPixelMoveTime = (1 / moveStep) / moveSpeed;
                        targetPos = targetCollider.ClosestPoint(controller.transform.position);//.transform.position;
                        targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                        flatTargetPosition = PositionGetter.GetFlatPos(targetPos);
                        Debug.Log($"自分{flatMyPosition}相手{flatTargetPosition}");
                        var direction = (targetPos - controller.transform.position).normalized;
                        var perTargetPos = PositionGetter.GetPerTargetPos(controller.transform.position, direction,flyingOffsetY);
                        var perDirection = perTargetPos - controller.transform.position;
                        var targetRot = Quaternion.LookRotation(perDirection);
                        controller.transform.rotation = targetRot;

                        moveTween = controller.transform.DOMove(perTargetPos, perPixelMoveTime);
                        moveTask = moveTween.ToUniTask(cancellationToken: cts.Token);

                        Debug.DrawLine(controller.transform.position, flatTargetPosition, Color.yellow); //で目視確認

                        while (!moveTask.Status.IsCompleted() && !cts.IsCancellationRequested)
                        {
                            var isDead = controller.isDead;
                            flatMyPosition = PositionGetter.GetFlatPos(controller.transform.position);
                            //var simpleDistance = Vector3.Distance(controller.transform.position, targetPos);
                            //var correntDistance = simpleDistance - myRadius;
                            if (isDead) { cts?.Cancel();  break; }
                            if (Vector3.Distance(flatMyPosition, flatTargetPosition) <= controller.MonsterStatus.AttackRange
                              || targetEnemy == null || controller.isKnockBacked_Spell)
                            {
                                Debug.Log("敵に到着 || ターゲットが範囲外にいきました");

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
                    //var targetCollider = target.GetComponent<Collider>();
                  
                    Debug.Log("タワーを追跡中");

                    if (target == null || targetCollider == null) return;
                    //targetPos = targetCollider.ClosestPoint(controller.transform.position);
                    //targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                    Debug.Log(targetPos);
                      
                        while (Vector3.Distance(controller.transform.position, targetPos) > controller.MonsterStatus.AttackRange
                         && target != null)
                        {
                            var perPixelMoveTime = (1 / moveStep) / moveSpeed;

                            targetPos = targetCollider.ClosestPoint(controller.transform.position);
                            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
                            var direction = (targetPos - controller.transform.position).normalized;
                            Debug.Log(target.gameObject.name);
                            var perTargetPos = PositionGetter.GetPerTargetPos(controller.transform.position, direction,flyingOffsetY);

                            var perDirection = perTargetPos - controller.transform.position;
                            var targetRot = Quaternion.LookRotation(perDirection);
                            controller.transform.rotation = targetRot;

                            moveTween = controller.transform.DOMove(perTargetPos, perPixelMoveTime);
                            moveTask = moveTween.ToUniTask(cancellationToken: cts.Token);//一歩分の動き

                            while (!moveTask.Status.IsCompleted() && !cts.IsCancellationRequested)
                            {
                                var isDead = controller.isDead;
                            　　　
                                if (isDead) { cts?.Cancel(); break; }

                                if (Vector3.Distance(controller.transform.position, targetPos) <= controller.MonsterStatus.AttackRange
                                      || controller.isKnockBacked_Spell)
                                {
                                    Debug.Log("敵に到着");
                                    cts?.Cancel();
                                    SetAttackStateField(target, targetPos);
                                    break;
                                }
                                await UniTask.Yield();
                            }
                            //キャンセルしても上のTaskはawaitしていないからここまで進み、下のawait moveTaskに到達するとエラーが出てしまうのでここでbreak
                            if (cts.IsCancellationRequested)
                            {
                                Debug.Log("ノックバックされました");
                                moveTween.Kill();
                                break;
                            }
                            //基本的にはここまでキャンセルされず到達する
                            await moveTask;
                        }
                }
            }
            finally
            {
                Debug.Log("おおかかｃｄｈｃｄｓｈかしあｊか");
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
        }      
     
        void EvaluateNewTargetAndChase()
        {
            if (myMonsterAttackType == MonsterAttackType.OnlyBuilding)
            {
                SetTargetTower();
                if (targetTower != null && !isChasing)
                {

                    try
                    {
                        cts?.Cancel();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Debug.LogWarning("すでにDisposeされてるため、Cancelはスキップされました: " + ex.Message);
                    }
                    ChaseTarget().Forget();
                }
                return;
            }
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(controller.gameObject, controller.MonsterStatus.ChaseRange);

            var myType = controller.moveType;
            var effecttiveSide = myType switch
            {
                MoveType.Walk => MoveType.Walk,
                MoveType.Fly => MoveType.Fly | MoveType.Walk,
                _ => default
            };

            var filterdArray = sortedArray.Where(cmp =>
            {
                var enemySide = cmp.GetUnitSide(controller.ownerID);
                var isDead = cmp.isDead;
                var moveType = cmp.moveType;
             
                if(cmp.TryGetComponent<ISummonbable>(out var summonbable))
                {
                    var isSummoned = summonbable.isSummoned;
                    return enemySide != Side.PlayerSide && !isDead
                      && (moveType & effecttiveSide) != 0 && isSummoned;// 
                }
                return enemySide != Side.PlayerSide && !isDead
                        && (moveType & effecttiveSide) != 0;// 
            }).ToArray();

            if (filterdArray.Length == 0)
            {
                targetEnemy = null;
                if(targetTower != null && !isChasing) ChaseTarget().Forget();
            }
            else
            {
                var newTarget = filterdArray[0].gameObject;
                //同じ敵なら何もしない
                if ((targetEnemy == newTarget || targetTower == newTarget)
                    && isChasing) return;
                Debug.Log("ターゲット変更");
                targetEnemy = newTarget;

                //Dispose()されたのが来ないようにするやり方がわからなかった、逃げ
                try
                {
                    cts?.Cancel();
                }
                catch (ObjectDisposedException ex)
                {
                    Debug.LogWarning("すでにDisposeされてるため、Cancelはスキップされました: " + ex.Message);
                }
                ChaseTarget().Forget();
            }
        }
    }
}

