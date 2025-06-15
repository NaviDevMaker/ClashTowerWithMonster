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
            addForceToUnit = new AddForceToUnit<SpellBase>(this, SpellStatus.PushAmount, SpellStatus.PerPushDurationAndStunTime);
        }
        protected override async UniTaskVoid Spell()
        {
            Debug.Log("�t�@�C�A�����I�I�I�I�I�I�I�I�I");
            addForceToUnit.KeepDistance(moveType);
            spellDamageHelper.DamageToUnit();
            particle.Play();
            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
            particle.Stop();
            Destroy(gameObject);
        }
    }
}


