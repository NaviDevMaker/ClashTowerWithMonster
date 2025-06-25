using Cysharp.Threading.Tasks;
using Game.Spells;
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
            addForceToUnit = new AddForceToUnit<SpellBase>(this, _SpellStatus.PushAmount, _SpellStatus.PerPushDurationAndStunTime);
        }
        protected override void SetDuration()
        {
            boumndShieldEffect = GetComponent<VisualEffect>();
            spellDuration = 10f;
        }

        protected override async UniTaskVoid Spell()
        {
            var time = 0f;
            boumndShieldEffect.Play();
            while (time < spellDuration)
            {
                time += Time.deltaTime;
                addForceToUnit.KeepDistance(moveType);
                await UniTask.Yield();
            }


            boumndShieldEffect.Stop();
            while (boumndShieldEffect.HasAnySystemAwake())
            {
                await UniTask.Yield();
            }

            Destroy(gameObject);
        }
    }

}

