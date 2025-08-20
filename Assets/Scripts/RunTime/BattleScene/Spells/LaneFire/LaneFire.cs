using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;
using UnityEngine;


namespace Game.Spells.LaneFire
{
    public class LaneFire : SpellBase
    {
        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/LaneSpell");
            base.Initialize();
            pushEffectUnit = PushEffectUnit.OnlyEnemyUnit;
            addForceToUnit = new AddForceToUnit<SpellBase>(this, _SpellStatus.PushAmount, _SpellStatus.PerPushDurationAndStunTime, pushEffectUnit);
        }

        protected override void SetDuration() => spellDuration = _SpellStatus.SpellDuration;

        protected override async UniTaskVoid Spell()
        {
            try
            {
                var particle = transform.GetChild(0).GetComponent<ParticleSystem>();
                this.particle = particle;
                var main = this.particle.main;
                var destroyTime = main.duration;

                await UniTask.Delay(TimeSpan.FromSeconds(spellDuration), cancellationToken: this.GetCancellationTokenOnDestroy());
                Debug.Log("ファイア発動！！！！！！！！！");
                addForceToUnit.KeepDistance(moveType);
                spellEffectHelper.EffectToUnit();
                particle.Play();
                await UniTask.Delay(TimeSpan.FromSeconds(destroyTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { return; }
            //particle.Stop();
            DestroyAll();
        }

        protected override async void DestroyAll()
        {
            if (particle == null) return;
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            await task;
            if (this == null) return;
            Destroy(gameObject);
        }
    }
}


