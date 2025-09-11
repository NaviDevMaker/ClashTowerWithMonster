using Cysharp.Threading.Tasks;
using Game.Monsters.Salamander;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Monsters.Salamander
{
    public class FireMover : LongDistanceAttack<SalamanderController>,IHitEffectDisapaer
    {
        public bool IsProcessingTask { get; private set;} = false;
        Dictionary<ParticleSystem,ParticleSystem.MainModule> pAndm = new Dictionary<ParticleSystem,ParticleSystem.MainModule>();
        Dictionary<ParticleSystem, float> originalSimuSpeeds = new Dictionary<ParticleSystem, float>();
        protected override void Update()
        {
            if (IsReachedTargetPos)
            {
                if (target != null && !target.isDead)
                {
                    Debug.Log($"{target}に到達しました、ダメージを与えます");
                    var currentTarget = target;
                    DamageToEnemy(currentTarget);
                    EffectManager.Instance.hitEffect.GenerateHitEffect(currentTarget);
                    OnEndProcess?.Invoke(this);
                }
            }
        }
        protected override void Initialize(SalamanderController attacker)
        {
            var particles = GetComponentsInChildren<ParticleSystem>().ToList();
            particles.ForEach(p =>
            {
                pAndm[p] = p.main;
                originalSimuSpeeds[p] = p.main.simulationSpeed;
            });
            base.Initialize(attacker);
        }
        public override void Move()
        {
            IsProcessingTask = true;
            for (var i = 0; i < pAndm.Count; i++)
            {
                var keyAndValue = pAndm.ElementAt(i);
                var main = keyAndValue.Value;
                var particle = keyAndValue.Key;
                main.loop = true;
                particle.Play();              
            }
            transform.SetParent(null);
            base.Move();
        }
        public async UniTask WaitEffectDissapaer()
        {
            var particles = pAndm.Keys.ToList();
            var tasks = particles.Select(p =>
            {
                if (p == null) return UniTask.CompletedTask;
                var main = pAndm[p];
                main.loop = false;
                main.simulationSpeed *= 1.5f;
                var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
                return task;
            });
            await tasks;
            particles.ForEach(p =>
            {
                if (p == null) return;
                var main = pAndm[p];
                var originalSpeed = originalSimuSpeeds[p];
                main.simulationSpeed = originalSpeed;
            });
            IsProcessingTask = false;
        }
    }
}


