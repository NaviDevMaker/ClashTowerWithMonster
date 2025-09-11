using Game.Monsters.EvilMage;
using UnityEngine;
using System.Collections;
using UnityEngine.VFX;
using UnityEngineInternal;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Events;

public interface IHitEffectDisapaer
{
    UniTask WaitEffectDissapaer();
}

namespace Game.Monsters.EvilMage
{
    public class MageSpellMover : LongDistanceAttack<EvilMageController>,IHitEffectDisapaer
    {
        VisualEffect visualEffect;
        ParticleSystem smokeParticle;
        ParticleSystem.MainModule mainModule;
        public bool IsProcessingTask { get; private set;} = false;
        protected override void Update()
        {
            if (IsReachedTargetPos)            
            {
                Func<UniTask> mageAttackHitEffect = () => EffectManager.Instance.hitEffect.GenerateMageAttackHitEffect(transform.position,target);
                if(target != null && !target.isDead)
                {
                    Debug.Log(target);
                    var currentTarget = target;
                    DamageToEnemy(currentTarget,mageAttackHitEffect);
                    EffectManager.Instance.hitEffect.GenerateHitEffect(currentTarget);
                    OnEndProcess?.Invoke(this);
                }         
            }
        }
        public override void Move()
        {
            IsProcessingTask = true;
            mainModule.loop = true;
            visualEffect.Play();
            smokeParticle.Play();
            transform.SetParent(null);
            base.Move();
        }
        protected override void Initialize(EvilMageController attacker)
        {
            base.Initialize(attacker);
            visualEffect = GetComponent<VisualEffect>();    
            smokeParticle = GetComponentInChildren<ParticleSystem>();
            mainModule = smokeParticle.main;
        }    
        public async UniTask WaitEffectDissapaer()
        {
            Func<UniTask> visualEffectDisapaer = async () =>
            {
                try
                {
                    visualEffect.Stop();
                    await UniTask.WaitUntil(() => visualEffect.aliveParticleCount == 0, cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                catch (OperationCanceledException) { }
            };
            mainModule.loop = false;
            var task = visualEffectDisapaer();
            var task2 = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(smokeParticle);
            await UniTask.WhenAll(task, task2);
            IsProcessingTask = false;
        }
    }
}


