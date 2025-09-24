using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using static UnityEngine.InputManagerEntry;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Game.Monsters.StingRay
{
    public class AttackState : AttackStateBase<StingRayController>, IEffectSetter
    {
        //Windのオフセット(0,地面の高さ、0.5)でヒットコライダーの子供につけるようにして
        public AttackState(StingRayController controller) : base(controller)
        {
            SetEffect();
        }

        Dictionary<VisualEffect,Vector3> poolWindVfxs = new Dictionary<VisualEffect, Vector3>();
        VisualEffect windPrefab;
        Queue<VisualEffect> windQueue = new Queue<VisualEffect>();
        int generateCount = 5;
        Vector3 windOffSet = new Vector3(0f,0f,0.5f);
        bool isWinding = false;
        SimpleAttackArguments arguments;
        public override void OnEnter()
        {
           
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f)
            {
                StateFieldSetter.AttackStateFieldSet<StingRayController>(controller, this, clipLength, 23,
                                    controller.MonsterStatus.AttackInterval);
                arguments = new SimpleAttackArguments
                {
                    getTargets = () => controller.GetUnitInWeponRange().Invoke(),
                    attackEffectAction = () =>
                    {
                        AbsorptionUnit();
                        PlayWindEffect();
                        isWinding = true;
                    },
                    attackEndAction = () =>
                    {
                        StopCurrentWind();
                        isWinding = false;
                    }
                };
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        protected override async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        {
           
            var now = 0f;
            try
            { 
                if (!controller.statusCondition.Freeze.isActive) LookToTarget();
                await UniTask.WaitUntil(() =>
                {
                    if (controller.isDead) return false;
                    return controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName);
                },                      cancellationToken: cts.Token);
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
                arguments.attackEffectAction.Invoke();
           
                var remaining = controller.repeatCount;
                var repeatInterval = controller.animator.GetRepeatInterval(startNormalizeTime, remaining
                                                                      ,clipLength,stateAnimSpeed);


                while (remaining > 0 && controller.animator.GetCurrentNormalizedTime(startNormalizeTime) < 1.0f 
                    && !cts.IsCancellationRequested && !isInterval)
                {
                    repeatInterval = controller.animator.GetRepeatInterval(startNormalizeTime, remaining
                                                                      , clipLength, stateAnimSpeed);
                    var currentTargets = arguments.getTargets();
                    currentTargets.ForEach(target =>
                    {
                        AddDamageToTarget(target);
                    });
                    remaining--;
                    await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval), cancellationToken: cts.Token);
                }
            }        
            catch (OperationCanceledException)
            {
                var elapsedTime = (now - startNormalizeTime) * clipLength;
                leftLengthTime = Mathf.Max(0f, clipLength - elapsedTime / stateAnimSpeed);
                isAttacking = false;
            }
            catch (ObjectDisposedException) { return; }
            finally { arguments.attackEndAction.Invoke(); }
            if(!cts.IsCancellationRequested) leftLengthTime = 0f;
        }
        void PlayWindEffect()
        {
            var wind = GetCurentWind();
            if (wind != null)
            {
                windQueue.Enqueue(wind);
                wind.SetFloat("SwirlRotationSpeed", -20f);
                var pos = wind.transform.position;
                pos.y = Terrain.activeTerrain.SampleHeight(pos);
                wind.transform.position = pos;
                var dust = wind.GetComponentsInChildren<VisualEffect>()
                    .FirstOrDefault(vfx => vfx != null && vfx.gameObject != wind.gameObject);
                Debug.Log(dust);
                dust.gameObject.SetActive(true);
                if (dust.playRate != 4.0f) dust.playRate = 4.0f;
                wind.Play();
                dust.Play();
            }
        }

        async void StopCurrentWind()
        {
            if (windQueue.Count == 0) return;
            var currentWind = windQueue.Dequeue();

            Func<UniTask> changeRotateSpeed = async () =>
            {
                var decreaseInterval = 0.1f;
                var targetValue = -5f;
                try
                {
                    while (currentWind.aliveParticleCount != 0 && currentWind.GetFloat("SwirlRotationSpeed") < targetValue)
                    {
                        var currentTargetValue = currentWind.GetFloat("SwirlRotationSpeed") + 1f;
                        currentWind.SetFloat("SwirlRotationSpeed", currentTargetValue);
                        await UniTask.Delay(TimeSpan.FromSeconds(decreaseInterval)
                            , cancellationToken: currentWind.GetCancellationTokenOnDestroy());
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    currentWind.SetFloat("SwirlRotationSpeed", targetValue);
                }
              
            };

            Func<VisualEffect, UniTask> waitVisual = async (vfx) =>
            {
                try
                {
                    vfx.Stop();
                    while (vfx.aliveParticleCount != 0)
                    {
                        await UniTask.Yield(cancellationToken:vfx.GetCancellationTokenOnDestroy());
                    }
                }
                catch (OperationCanceledException) { }
                finally
                {
                    if (vfx != null) vfx.gameObject.SetActive(false);
                }
            };
            changeRotateSpeed().Forget();
            var currentDust = currentWind.GetComponentsInChildren<VisualEffect>()
                             .FirstOrDefault(vfx => vfx.gameObject != currentWind.gameObject); ;
            var task = waitVisual(currentWind);
            var task2 = waitVisual(currentDust);
            await UniTask.WhenAny(task, task2);
            if (currentWind != null)
            {
                try
                {
                    var duration = 0.25f;
                    var scaleSet = new Vector3TweenSetup(Vector3.zero, duration);
                    await currentWind.gameObject.Scaler(scaleSet)
                        .ToUniTask(cancellationToken:currentWind.GetCancellationTokenOnDestroy());
                }
                catch (OperationCanceledException) { return; }
                currentWind.gameObject.SetActive(false);
            }
        }
        public async void SetEffect()
        {
            var windObj = await SetFieldFromAssets.SetField<GameObject>("Monsters/StingRayWindVFX");
            windPrefab = windObj.GetComponent<VisualEffect>();
            var rangeAttackObj = controller.rangeAttackObj;
            var pos = rangeAttackObj.transform.position;
            for (int i = 0; i < generateCount; i++)
            {
                var wind = UnityEngine.Object.Instantiate(windPrefab, pos, Quaternion.identity);
                wind.playRate = 6.0f;
                wind.transform.SetParent(rangeAttackObj.transform);
                wind.transform.localPosition = Vector3.zero;
                wind.transform.localPosition += windOffSet;
                poolWindVfxs[wind] = wind.transform.localScale;
                wind.gameObject.SetActive(false);
            }
        }

        VisualEffect GetCurentWind()
        {
            var currentWind = default(VisualEffect);
            for (int i = 0; i < poolWindVfxs.Count; i++)
            {
                var wind = poolWindVfxs.ElementAt(i).Key;
                if (wind.aliveParticleCount == 0 && !wind.gameObject.activeSelf)
                {
                    currentWind = wind;
                    currentWind.transform.localScale = poolWindVfxs[currentWind];
                    return currentWind;
                }
            }

            if(currentWind == null)
            {
                var rangeAttackObj = controller.rangeAttackObj;
                var pos = rangeAttackObj.transform.position;
                currentWind = UnityEngine.Object.Instantiate(windPrefab, pos, Quaternion.identity);
                currentWind.transform.SetParent(rangeAttackObj.transform);
                currentWind.transform.localPosition = Vector3.zero;
                currentWind.transform.localPosition += windOffSet;
            }

            return currentWind;
        }

        protected override bool CheckAttackable()
        {
            var canAttack = false;
            if (isWinding) return true;
            if(target == null) return false;
            var targetPos = Vector3.zero;
            var isDead = target.isDead;
            var isTransparent = target.statusCondition.Transparent.isActive;
            var isNonTarget = target.statusCondition.NonTarget.isActive;
            var collider = target?.GetComponent<Collider>();
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

            canAttack = (targetPos - myPos).magnitude <= attackRange
                && !isTransparent && !isNonTarget &&(targetSide & effectiveSide) != 0 && !isDead;// && !isDead;         
            Debug.Log($"[距離チェック] 距離: {canAttack}, 射程: {controller.MonsterStatus.AttackRange}");
            return canAttack;
        }
        async void AbsorptionUnit()
        {
            Func<bool> canAbsorb = () =>
            {
                try
                {
                    var cancelled = cts.IsCancellationRequested;
                    var isFreezed = controller.statusCondition.Freeze.isActive;
                    var isDeadMine = controller.isDead;
                    return !cancelled && !isFreezed && !isInterval && !isDeadMine;
                }
                catch (ObjectDisposedException) { return false; }
            };

            UnityAction<UnitBase> absorptionAction = (target) =>
            {
                   if (target == null) return;
                   var isFreezed = target.statusCondition.Freeze.isActive;
                   if (target is ITower || isFreezed) return;
                   var rangeAttackPos = controller.rangeAttackObj.transform.position;
                   var absorptionDistance = 0.1f;
                   var flatPos_me = PositionGetter.GetFlatPos(rangeAttackPos);
                   var vector = flatPos_me - target.transform.position;
                   var distance = vector.magnitude;
                   var absorptionDir = vector.normalized;
                   var absorptionAmount = Mathf.Min(distance, absorptionDistance);
                   var targetPos = target.transform.position + absorptionDir * absorptionAmount;
                   var duration = 0.1f;
                   var moveSet = new Vector3TweenSetup(targetPos, duration, Ease.Linear);
                   target.gameObject.Mover(moveSet);
                   
            };
           
            var previousTargets = new List<UnitBase>(); 
            var currentTargets = new List<UnitBase>();
            try
            {
                while (canAbsorb())
                {
                    currentTargets = controller.GetUnitInWeponRange().Invoke();
                    previousTargets.ForEach(preTarget =>
                    {
                        if (preTarget == null) return;
                        if (!currentTargets.Contains(preTarget))
                        {
                            preTarget.statusCondition.Absorption.isActive = false;
                        }
                    });
                    currentTargets.ForEach(target =>
                    {
                        if (target is ITower) return;
                        target.statusCondition.Absorption.isActive = true;
                        absorptionAction(target);
                    });
                    previousTargets = currentTargets;
                    await UniTask.Yield(cancellationToken: cts.Token);
                }
            }
            catch(ObjectDisposedException){ }
            finally
            {
                var hashUnits = new HashSet<UnitBase>();
                hashUnits.AddRange(currentTargets);
                hashUnits.AddRange(previousTargets);
                hashUnits.ToList().ForEach(unit =>
                {
                    unit.statusCondition.Absorption.isActive = false;
                });
            }
        }
    }  
}