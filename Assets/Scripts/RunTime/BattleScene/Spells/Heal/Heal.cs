using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;


namespace Game.Spells.Heal
{
    public class Heal : SpellBase
    {
        List<ParticleSystem> particleList = new List<ParticleSystem>();
        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/Heal");
            base.Initialize();
            particle = GetComponent<ParticleSystem>();
        }
        protected override void SetRange()
        {
            scaleAmount = 5f;
            base.SetRange();
        }

        protected override void SetDuration()
        {
            spellDuration = _SpellStatus.SpellDuration;
        }
        protected override async UniTaskVoid Spell()
        {
            var interval = 0.5f;
            var time = 0f;
            var intervalCount = 0f;
            particle.Play();
            while (time < spellDuration)
            {
                time += Time.deltaTime;
                intervalCount += Time.deltaTime;
                if (intervalCount >= interval)
                {
                    spellEffectHelper.EffectToUnit();
                    intervalCount = 0f;
                }
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            foreach (Transform child in transform)
            {
                var p = child.GetComponent<ParticleSystem>();
                if (p != null) particleList.Add(p);
            }
            particleList.ForEach((p) =>
            {
                var main = p.main;
                main.loop = false;
            });
            DestroyAll();
        }

        protected override async void DestroyAll()
        {
            try
            {
                var tasks = new List<UniTask>();
                particleList.ForEach(p => tasks.Add(AwaitUntilNoExistingParticle(p)));
                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException) { return; }
            if (this == null) return;
            Destroy(particle.gameObject);
        }

        async UniTask AwaitUntilNoExistingParticle(ParticleSystem paerticle)
        {
            try
            {
                 while (particle.IsAlive())
                 {
                    await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                 }
            }
            catch (OperationCanceledException) {throw; }        
        }
    }
}


