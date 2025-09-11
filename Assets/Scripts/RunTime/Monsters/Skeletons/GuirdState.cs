using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Game.Monsters.Skeleton
{
    public class GuirdState : StateMachineBase<SkeletonController>,IEffectSetter
    {
        public GuirdState(SkeletonController controller) : base(controller) 
        {
            SetEffect();
        }
        public readonly int guird_Hash = Animator.StringToHash("isGuirding");
        ParticleSystem shieldEffect;
        public override void OnEnter()
        {
            controller. guirdInfo.isGuirding = true;
            nextState = controller.ChaseState;
            controller.animator.SetTrigger(guird_Hash);
            Guird();
        }
        public override void OnExit() { }
        public override void OnUpdate()
        {
            if(!controller.guirdInfo.isGuirding) controller.ChangeState(nextState);
        }
        async void Guird()
        {
            var parentObj = default(GameObject);
            var particles = Enumerable.Empty<ParticleSystem>().ToList();
            try
            {
                await UniTask.WaitUntil(() =>controller.animator.GetCurrentAnimatorStateInfo(0).IsName("Guird")
                ,cancellationToken:controller.GetCancellationTokenOnDestroy());

                PlayShieldEffect(out particles,out parentObj);
                await UniTask.Delay(TimeSpan.FromSeconds(controller.guirdInfo.guirdDuration)
                ,cancellationToken:controller.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { }
            finally
            {
                DestoryEffect(particles,parentObj);
                controller.guirdInfo.isGuirding = false;
            }
        }
        public async void SetEffect()
        {
            var shieidObj = await SetFieldFromAssets.SetField<GameObject>("Effects/SkeletonShieidEffect");
            if (shieidObj == null) return;
            shieldEffect = shieidObj.GetComponent<ParticleSystem>();
        }
        void PlayShieldEffect(out List<ParticleSystem> shieldParticles,out GameObject parentParticleObj)
        {
            var offsetZ = controller.transform.forward * 0.25f;
            var pos = controller.guirdInfo.ShieldMesh.bounds.center + offsetZ;
            var rot = controller.transform.rotation * shieldEffect.transform.rotation;
            var shield = UnityEngine.Object.Instantiate(shieldEffect, pos, rot);
            shield.gameObject.transform.SetParent(controller.transform);
            parentParticleObj = shield.gameObject;
            var duration = controller.guirdInfo.guirdDuration;
            var particles = shield.GetComponentsInChildren<ParticleSystem>().ToList();
                particles.ForEach(p =>
                {
                    var main = p.main;
                    main.duration = duration;
                    main.startLifetime = duration;
                    p.Play();
                });
            shieldParticles = particles;
        }
        async void DestoryEffect(List<ParticleSystem> particles,GameObject parent)
        {
            parent.transform.SetParent(null);
            var tasks = particles.Select(p =>
            {
                var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
                return task;
            }).ToArray();
            await tasks;
            particles.ForEach(p => UnityEngine.Object.Destroy(p.gameObject));
        }
    }
}


