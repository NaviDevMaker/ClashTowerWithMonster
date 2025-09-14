using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.Cyclops
{
    public class AttackState : AttackStateBase<CyclopsController>,IEffectSetter
    {
        public AttackState(CyclopsController controller) : base(controller)
        {
            SetEffect();
        }
        ParticleSystem beamEffect;
        float beamMotionzEndNorm = 0f;
        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f)
            { 
                StateFieldSetter.AttackStateFieldSet<CyclopsController>(controller, this, clipLength, 14,
                                controller.MonsterStatus.AttackInterval);
                var targetClipName = controller.MonsterAnimPar.attackAnimClipName + "_PlusEvent";
                var beamEndFrame = 45;
                beamMotionzEndNorm = (float)beamEndFrame / (float)maxFrame;
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
            var arguments = new SimpleAttackArguments
            { 
                getTargets = attackArguments.getTargets,
                attackEffectAction = StartBeamAttack,
                repeatCount = controller.ContinuousAttackMonsterStatus._ContinuousAttackInfo.ContinuousCount
            };

            await base.Attack_Generic(arguments);
        }

        protected override void AddDamageToTarget(UnitBase currentTarget)
        {
            Debug.Log("ダメージを与えます");
            if (GetCurrentNormalizedTime() >= beamMotionzEndNorm) return;
            base.AddDamageToTarget(currentTarget);
        }
        async void StartBeamAttack()
        {
            Debug.Log("ビーム発射!!!!!");
            var doubleCls = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, controller.GetCancellationTokenOnDestroy());
            var rotOffset = Quaternion.Euler(178f,-18f,0f);
            var pos = controller.BeamHand.position;
            var beam = UnityEngine.Object.Instantiate(beamEffect,pos,Quaternion.identity);
            var main = beam.main;
            main.loop = true;
            beam.Play();
            Func<bool> canEmitBeam = () =>
            {
                var cancelled = doubleCls.IsCancellationRequested;
                var isFreezed = controller.statusCondition.Freeze.isActive;
                var isDead = target.isDead;

                var isInBeamEmitNorm = GetCurrentNormalizedTime() < beamMotionzEndNorm;
                return !cancelled && !isFreezed && !isDead && !isInterval && isInBeamEmitNorm;
            };

            try
            {
                var maxScale = new Vector3(1f, 1f, 0.1f);
                beam.transform.localScale = new Vector3(1.0f,1.0f,0f);
                while (canEmitBeam())
                {
                    LookToTarget();
                    var center = target.BodyMesh.bounds.center;
                    var direction = (center - beam.transform.position).normalized;
                    var rot = Quaternion.LookRotation(direction);
                    var distance = (center - controller.BeamHand.position).magnitude;
                    var lerp = Mathf.Clamp01(distance / controller.MonsterStatus.AttackRange);
                    var currentScale = beam.transform.localScale;
                    var z = Mathf.Lerp(currentScale.z, maxScale.z, lerp);
                    var targetScale = new Vector3(currentScale.x, currentScale.y, z);
                    beam.transform.localScale = targetScale;
                    beam.transform.localRotation = rot;
                    await UniTask.Yield(cancellationToken: doubleCls.Token);
                }
            }
            catch(OperationCanceledException){ }     
            finally
            {
                if(beam != null) UnityEngine.Object.Destroy(beam.gameObject);
            }
        }
        public async void SetEffect()
        {
            if (beamEffect != null) return;
            var beamObj = await SetFieldFromAssets.SetField<GameObject>("Effects/BeamEffect");
            beamEffect = beamObj.GetComponent<ParticleSystem>();
        }
    }
}