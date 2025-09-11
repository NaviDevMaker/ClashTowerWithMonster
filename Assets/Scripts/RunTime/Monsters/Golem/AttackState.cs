using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Monsters.Golem
{
    public class AttackState : AttackStateBase<GolemController>,IEffectSetter
    {
        public AttackState(GolemController controller) : base(controller)
        {
            SetEffect();
        }

        ParticleSystem tornadoEffect;
        ParticleSystem smokeEffect;

        ParticleSystem currentTornado;
        ParticleSystem currentSmoke;
        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<GolemController >(controller, this, clipLength,22,
                controller.MonsterStatus.AttackInterval);
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
            var arguments = new SimpleAttackArguments
            { 
               getTargets = attackArguments.getTargets,
               attackEffectAction = PlayEffects,
               specialEffectAttack = Absorption
            };
            await base.Attack_Generic(arguments);
            if(currentTornado != null) DestroyParticle();
        }
        async void Absorption(UnitBase target) 
        {
            try
            {
                if (target is TowerController) return;
                var absorptionDistance = 5f;
                var collider = controller.GetComponent<Collider>();
                var flatPos_target = PositionGetter.GetFlatPos(target.transform.position);
                var closestPos = collider.ClosestPoint(flatPos_target);
                var flatPos_me = PositionGetter.GetFlatPos(closestPos);
                var vector = flatPos_me - flatPos_target;
                var distance = vector.magnitude;
                if (distance <= 0.1f) return;
                var absorptionDir = vector.normalized;
                // 移動する距離は「実際の距離」か「吸収距離上限」の小さい方
                var absorptionAmount = Mathf.Min(distance, absorptionDistance);
                // 吸い寄せ先は「ターゲットの現在位置 + 吸収方向 * 距離」
                var targetPos = target.transform.position + absorptionDir * absorptionAmount;
                var duration = 0.5f;
                var moveSet = new Vector3TweenSetup(targetPos, duration, Ease.Linear);
                var moverTask = target.gameObject.Mover(moveSet)
                    .ToUniTask(cancellationToken: target.GetCancellationTokenOnDestroy());
                await moverTask;
            }
            catch (OperationCanceledException) { }
        }
        void PlayEffects()
        {
            if (tornadoEffect == null) return;
            var pos = controller.rangeAttackObj.transform.position;
            var tornado = UnityEngine.Object.Instantiate(tornadoEffect, pos, tornadoEffect.transform.rotation);
            var smoke = UnityEngine.Object.Instantiate(smokeEffect, pos, smokeEffect.transform.rotation);
            currentTornado = tornado;
            currentSmoke = smoke;
            tornado.Play();
            currentSmoke.Play();
        }
        public async void SetEffect()
        {
            var tornadoObj = await SetFieldFromAssets.SetField<GameObject>("Effects/GolemAttackTornado");
            var smokeObj = await SetFieldFromAssets.SetField<GameObject>("Effects/GolemAttackSmoke");
            tornadoEffect = tornadoObj.GetComponent<ParticleSystem>();
            smokeEffect = smokeObj.GetComponent<ParticleSystem>();
        }
        async void DestroyParticle()
        {
            Debug.Log("エフェクトを消します");
            if (currentTornado == null || currentSmoke == null) return;
            var expectedEffects = new List<ParticleSystem>{currentTornado,currentSmoke};
            var tasks = Enumerable.Empty<UniTask>().ToList();
            expectedEffects.ToList().ForEach(p => tasks.Add(RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p)));
            await UniTask.WhenAny(tasks);
            expectedEffects.ForEach(p =>
            {
                if (p == null) return;
                UnityEngine.Object.Destroy(p.gameObject);
            });
        }
    }
}