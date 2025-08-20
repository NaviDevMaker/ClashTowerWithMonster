using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;


namespace Game.Spells.BoundShield
{
    public class BoundShield : SpellBase
    {
        VisualEffect boumndShieldEffect;

        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/BoundShield");
            base.Initialize();
            pushEffectUnit = PushEffectUnit.OnlyEnemyUnit;
            addForceToUnit = new AddForceToUnit<SpellBase>(this, _SpellStatus.PushAmount, _SpellStatus.PerPushDurationAndStunTime,pushEffectUnit);
        }
        protected override void SetDuration()
        {
            boumndShieldEffect = GetComponent<VisualEffect>();
            spellDuration = _SpellStatus.SpellDuration;
        }

        protected override void SetRange()
        {
            scaleAmount = 5f;
            base.SetRange();
        }
        protected override async UniTaskVoid Spell()
        {
            var time = 0f;
            boumndShieldEffect.Play();
            try
            {
                while (time < spellDuration)
                {
                    time += Time.deltaTime;
                    addForceToUnit.KeepDistance(moveType);
                    await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                }


                boumndShieldEffect.Stop();
                while (boumndShieldEffect.HasAnySystemAwake())
                {
                    await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                }

            }
            catch (OperationCanceledException) { return; }

           if(this == null) Destroy(gameObject);
        }
    }

}

