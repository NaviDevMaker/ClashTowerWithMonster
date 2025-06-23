using Cysharp.Threading.Tasks;
using System;
using UnityEngine;


namespace Game.Spells.LaneFire
{
    public class LaneFire : SpellBase
    {
        protected override void Initialize()
        {
            base.Initialize();
            pushEffectUnit = PushEffectUnit.OnlyEnemyUnit;
            addForceToUnit = new AddForceToUnit<SpellBase>(this, SpellStatus.PushAmount, SpellStatus.PerPushDurationAndStunTime);
        }
        protected override async UniTaskVoid Spell()
        {
            Debug.Log("ファイア発動！！！！！！！！！");
            addForceToUnit.KeepDistance(moveType);
            spellEffectHelper.EffectToUnit();
            particle.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
            particle.Stop();
            Destroy(gameObject);
        }
    }
}


