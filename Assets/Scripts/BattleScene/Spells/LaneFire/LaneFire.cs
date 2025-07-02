using Cysharp.Threading.Tasks;
using System;
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
            addForceToUnit = new AddForceToUnit<SpellBase>(this, _SpellStatus.PushAmount, _SpellStatus.PerPushDurationAndStunTime);
        }
        protected override async UniTaskVoid Spell()
        {
            Debug.Log("ファイア発動！！！！！！！！！");
            addForceToUnit.KeepDistance(moveType);
            spellEffectHelper.EffectToUnit();
            particle.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
            //particle.Stop();
            DestroyAll();
        }

        protected override async void DestroyAll()
        {
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            await task;
            Destroy(gameObject);
        }
    }
}


