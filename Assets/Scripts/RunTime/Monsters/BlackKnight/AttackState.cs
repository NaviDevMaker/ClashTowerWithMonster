using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Unity.Burst.Intrinsics.Arm;

namespace Game.Monsters.BlackKnight
{
    public class AttackState : AttackStateBase<BlackKnightController>
    {
        public AttackState(BlackKnightController controller) : base(controller) { }

        ParticleSystem shockWaveEffect = null;
        ParticleSystem smokeEffect = null;
        GameObject shockWaveObj = null;
        GameObject smokeObj = null;

        AddForceToUnit<ShockWaveEffecter> addForce = null;
        public override void OnEnter()
        {
            SetEffects();
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<BlackKnightController >(controller, this, clipLength,25,
                controller.MonsterStatus.AttackInterval);
            if (addForce == null) addForce = controller.waveEffecter.AddForceToUnit;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        protected override async UniTask Attack_Generic(AttackArguments attackArguments)
        {
            var arguments = new AttackArguments
            {
                getTargets = attackArguments.getTargets,
                attackEffectAction = PlayShockWave,
                specialEffectAttack = (target) => addForce.CompareEachUnit(target)
            };
            
            PlaySmokeEffect(out smokeObj);
            try
            {
                await base.Attack_Generic(arguments);
            }
            finally
            {
                if (shockWaveObj != null && smokeObj != null) DestroyParticles(shockWaveObj,smokeObj);
            }
            leftLengthTime = 0f;
        }
        void PlayShockWave()
        {
            if (shockWaveEffect == null) 
            {
                shockWaveObj = null;
                return;
            }
            var wepon = controller.rangeAttackObj;
            var wavePos = target.transform.position;
            var offsetY = 0.5f;
            wavePos.y = Terrain.activeTerrain.SampleHeight(wavePos) + offsetY;
            var waveRot = shockWaveEffect.transform.rotation;         
            var shockWave = UnityEngine.Object.Instantiate(shockWaveEffect,wavePos,waveRot);         
            wepon.transform.position = wavePos;       
            shockWaveObj = shockWave.gameObject;
            shockWave.Play();
        }
        void PlaySmokeEffect(out GameObject smokeObj)
        {
            if(smokeEffect == null)
            {
                smokeObj = null;
                return;
            }
            var wepon = controller.rangeAttackObj;
            var weponMeshObj = wepon.transform.parent.gameObject.transform.GetChild(0).gameObject;
            var smokePos = weponMeshObj.transform.position;
            var smokeRot = smokeEffect.transform.rotation;
            var smoke = UnityEngine.Object.Instantiate(smokeEffect, smokePos, smokeRot);
            smoke.transform.SetParent(weponMeshObj.transform);
            var offset = Vector3.forward * 2.0f;
            var smokeDuration = clipLength / controller.MonsterStatus.AnimaSpeedInfo.AttackStateAnimSpeed;
            var main = smoke.main;
            main.duration = smokeDuration;

            smoke.transform.position += offset;
            smokeObj = smoke.gameObject;
            smoke.Play();
        }
        async void SetEffects()
        {
            if (shockWaveEffect != null && smokeEffect != null) return;
            Func<string, UniTask<ParticleSystem>> setEffectAction = async (adress)  =>
            {
                var obj = await SetFieldFromAssets.SetField<GameObject>(adress);
                if (obj == null) return null;
                var p = obj.GetComponent<ParticleSystem>();
                return p;
            };
            shockWaveEffect = await setEffectAction("Effects/ShockWaveEffect");
            smokeEffect = await setEffectAction("Effects/BlackNightWeponSmork");
        }
        async void DestroyParticles(GameObject shockWaveObj,GameObject smokeObj)
        {
            if (shockWaveObj == null || smokeObj == null) return;
            var p = shockWaveObj.GetComponent<ParticleSystem>();
            var p1 = smokeObj.GetComponent<ParticleSystem>();
            var task1 = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
            var task2 = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p1);
            await UniTask.WhenAll(task1,task2);
            if (p != null) UnityEngine.Object.Destroy(p.gameObject);
            if (p1 != null) UnityEngine.Object.Destroy(p1.gameObject);
        }
    }
}