using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.LizardWarrior
{
    public class DeathState : DeathStateBase<LizardWarriorController>
    {
        public DeathState(LizardWarriorController controller) : base(controller) { }
        public bool FireLingActionEnd { get; private set; } = false;
        GameObject fireLingObj = null;
        public override void OnEnter()
        {
            SummonFireRing();
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        async void SummonFireRing()
        {
            try
            {
                if(fireLingObj == null) fireLingObj = await SetFieldFromAssets.SetField<GameObject>("Effects/FireLingEffectParent");
                var flatPos = PositionGetter.GetFlatPos(controller.transform.position);
                var obj = UnityEngine.Object.Instantiate(fireLingObj, flatPos, Quaternion.identity);
                var particles = obj.GetComponentsInChildren<ParticleSystem>().ToList();
                particles.ForEach(p =>
                {
                    var main = p.main;
                    main.loop = true;
                    p.Play();
                });
                var scaleDuration = 0.25f;
                var targetScale = Vector3.one * 2.0f;
                var scaleUpSet = new Vector3TweenSetup(targetScale, scaleDuration, Ease.Linear);
                var scaleUpTask = obj.Scaler(scaleUpSet).ToUniTask(cancellationToken: controller.GetCancellationTokenOnDestroy());
                var tasks = Enumerable.Empty<UniTask>().ToList();
                tasks.AddRange(new UniTask[]{scaleUpTask,DamageInFireLingUnits(flatPos)});
                await UniTask.WhenAll(tasks);
                tasks.Clear();
                tasks.TrimExcess();
                tasks = particles.
                    Select(p =>
                    {
                        var main = p.main;
                        main.loop = false;
                        main.simulationSpeed *=  4.0f;
                        var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
                        return task;
                    }).ToList();
                await tasks;
                particles.ForEach(p =>
                {
                    if (p == null) return;
                    UnityEngine.Object.Destroy(p.gameObject);
                });
            }
            catch (OperationCanceledException) { }
        }
        async UniTask DamageInFireLingUnits(Vector3 pos)
        {
            var colliderObj = new GameObject("FireLingRangeCollider");
            colliderObj.transform.position = pos;
            var sphereCollider = colliderObj.AddComponent<SphereCollider>();
            sphereCollider.radius = 6.0f;
            controller.rangeAttackObj = colliderObj;
            Func<List<UnitBase>> getTargets = controller.GetUnitInWeponRange();
            var damageInterval = 0.5f;
            var duration = 7.0f;
            var startTime = Time.time;
            var elapsedTime = 0f;
            var damage = 10;

            try
            {
                while (Time.time - startTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime >= damageInterval)
                    {
                        elapsedTime = 0f;
                        var currentTargets = getTargets();
                        currentTargets.ForEach(target =>
                        {
                            if (target is IUnitDamagable damagable)
                            {
                                damagable.Damage(damage);
                            }
                        });
                    }
                    await UniTask.Yield(cancellationToken: controller.GetCancellationTokenOnDestroy());
                }
                FireLingActionEnd = true;
            }
            catch (OperationCanceledException) { }
            finally
            {
                if(colliderObj != null) UnityEngine.Object.Destroy(colliderObj);
            }      
        }
    }
}