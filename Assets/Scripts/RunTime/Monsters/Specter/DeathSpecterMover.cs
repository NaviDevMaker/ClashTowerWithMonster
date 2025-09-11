using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;



namespace Game.Monsters.Specter
{
    public class DeathSpecterMover : MonoBehaviour, IRangeAttack
    {
        public GameObject rangeAttackObj { get => throw new System.NotImplementedException();
                                           set => throw new System.NotImplementedException(); }
        float radius = 100.0f;
        float damageInterval = 0.25f;
        Animator animator;
        int myID = -1;
        int damage = 0;
        CancellationTokenSource specterCts = new CancellationTokenSource();
        MoveType moveType;
        public bool isReachedTargetPos { get; private set;} = false;
        public bool isDamageable { get; set; } = false;
        bool isDamaging = false;
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
        List<Renderer> renderers = new List<Renderer>();
        ParticleSystem ouraParticle;
        List<TrailRenderer> trailRenderers = new List<TrailRenderer>();
        readonly int chase_Hash = Animator.StringToHash("isChasing");
        private void Awake()
        {
            animator = GetComponent<Animator>();
            ouraParticle = transform.GetChild(2).GetComponent<ParticleSystem>();
            trailRenderers = GetComponentsInChildren<TrailRenderer>().ToList();
        }
        private void Update()
        {
            if (!isDamaging && !isReachedTargetPos && isDamageable) DamageEachUnit();
        }
        public void Initialize(SpecterController attacker)
        {
            renderers = GetComponentsInChildren<Renderer>().
                Where(renderer => !(renderer is ParticleSystemRenderer) && !(renderer is TrailRenderer))
                .ToList();
            renderers.ForEach(renderer =>
            {
                var mat = renderer.material;
                var color = mat.color;
                color.a = 0.5f;
                mat.color = color;
            });

            myID = attacker.ownerID;
            moveType = attacker.moveType;
            damage = attacker.MonsterStatus.AttackAmount * 2;
            ouraParticle.Play();
        }
        UnitBase GetTarget()
        {
            var soretedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(this.gameObject, radius);
            var filteredList = soretedArray.Where(target =>
            {
                var effectiveSide = Side.EnemySide;
                var effectiveMoveType = MoveType.Walk;
                var targetSide = target.GetUnitSide(myID);
                var isDead = target.isDead;
                var isTransparent = target.statusCondition.Transparent.isActive;
                if ((targetSide & effectiveSide) == 0 || isDead
                     || isTransparent || (moveType & effectiveMoveType) == 0) return false;
                if (target is ISummonbable summonbable)
                {
                    var isSummoned = summonbable.isSummoned;
                    if (!isSummoned) return false;
                }
                return true;
            }).ToList();

            if (filteredList.Count == 0)
            {
                return this.gameObject.GetTargetTower(myID);
            }
            else return filteredList[0];
        }
        async void MoveToTarget(UnitBase target)
        {
            if (isReachedTargetPos) return;
            specterCts = new CancellationTokenSource();
            var targetCollider = target.GetComponent<Collider>();
            var targetPos = targetCollider.ClosestPoint(transform.position);
            var flatTargetPos = PositionGetter.GetFlatPos(targetPos);
            var flatMyPosition = PositionGetter.GetFlatPos(transform.position);
            try
            {
                if (Vector3.Distance(flatMyPosition, flatTargetPos) <= Mathf.Epsilon)
                {
                    Debug.Log("既に範囲内にターゲットがいるので終了します");
                    return;
                }
                while (Vector3.Distance(target.BodyMesh.bounds.center,transform.position) >= Mathf.Epsilon)
                {
                    targetPos = targetCollider.ClosestPoint(transform.position);
                    flatTargetPos = PositionGetter.GetFlatPos(targetPos);
                    var direction = (flatTargetPos - transform.position).normalized;
                    var perTargetPos = PositionGetter.GetPerTargetPos(transform.position, direction);
                    var perDirection = perTargetPos - transform.position;
                    var rot = Quaternion.LookRotation(perDirection);
                    transform.rotation = rot;
                    var moveSet = new Vector3TweenSetup(perTargetPos, 0.1f, Ease.Linear);
                    var moveTask = this.gameObject.Mover(moveSet).ToUniTask(cancellationToken: specterCts.Token);
                 
                    while (!moveTask.Status.IsCompleted() && !specterCts.IsCancellationRequested)
                    {
                        var flatMyPos = PositionGetter.GetFlatPos(transform.position);
                        var isDead = target.isDead;
                        var isTransparent = target.statusCondition.Transparent.isActive;
                        if (isDead || isTransparent || target == null 
                            || Vector3.Distance(flatTargetPos,flatMyPos) <= Mathf.Epsilon)
                        {
                            specterCts?.Cancel();
                            var nextTarget = GetTarget();
                            MoveToTarget(nextTarget);
                            break;
                        }
                        await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                    }
                    if (specterCts.IsCancellationRequested)
                    {
                        specterCts?.Dispose();
                        break;
                    }
                    await moveTask;
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                var closestPos = targetCollider.ClosestPoint(transform.position);
                var flatOpponentPos = PositionGetter.GetFlatPos(closestPos);
                var flatMyPos = PositionGetter.GetFlatPos(transform.position);
                if (Vector3.Distance(flatOpponentPos,flatMyPos) <= Mathf.Epsilon)
                {
                    if(target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
                    {
                        unitDamagable.Damage(damage);
                    }
                    isReachedTargetPos = true;
                }
            }
        }
        async void DamageEachUnit()
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken:this.GetCancellationTokenOnDestroy());
                isDamaging = true;
                DamageCurrentTargets();
                await UniTask.Delay(TimeSpan.FromSeconds(damageInterval),
                    cancellationToken:this.GetCancellationTokenOnDestroy());
                isDamaging = false;
                semaphoreSlim.Release();
            }
            catch (OperationCanceledException){ }
        }
        public async void StartDeathSpecterAction()
        {
            animator.SetBool(chase_Hash, true);
            var startTarget = GetTarget();
            MoveToTarget(startTarget);
            await UniTask.WaitUntil(() => isReachedTargetPos, cancellationToken:this.GetCancellationTokenOnDestroy());
            await MoveToForward();
            animator.SetBool(chase_Hash, false);
            var fadeDuration = 3.0f;
            var allTasks = renderers.Select(renderer =>
            {
                var task = renderer.material.DOFade(0f,fadeDuration).ToUniTask(cancellationToken:this.GetCancellationTokenOnDestroy());
                return task;
            }).ToList();

            var main = ouraParticle.main;
            main.startLifetime = fadeDuration;
            main.loop = false;
            main.simulationSpeed /= 2f;

            var dissapearTask = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(ouraParticle);
            allTasks.Add(dissapearTask);
            await UniTask.WhenAll(allTasks);
            if (this != null) Destroy(this.gameObject);
        }
        async UniTask MoveToForward()
        {
            trailRenderers.ForEach(trail => trail.enabled = true);
            var duration = 0.5f;
            var damageInterval = 0.1f;
            var offsetZ = 5.0f;
            var targetPos = transform.position + transform.forward * offsetZ;
            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);
            var moveSet = new Vector3TweenSetup(targetPos, duration, Ease.Linear);
            var moveTween = gameObject.Mover(moveSet);
            var startTime = Time.time;
            try
            {             
                while (Time.time - startTime < duration && this != null)
                {           
                    DamageCurrentTargets();
                    await UniTask.Delay(TimeSpan.FromSeconds(damageInterval), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                moveTween.Kill();
            }
        }
        public void SetHitJudgementObject() => throw new System.NotImplementedException();

        void DamageCurrentTargets()
        {
            var getCurrentTargets = this.GetUnitInSpecificRangeItem(moveType: this.moveType, ownerID: this.myID).Invoke();
            getCurrentTargets.ForEach(target =>
            {
                if (target.TryGetComponent<IUnitDamagable>(out var unitDamagable))
                {
                    unitDamagable.Damage(damage);
                }
            });
        }
    }
}

