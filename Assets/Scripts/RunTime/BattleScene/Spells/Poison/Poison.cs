using Cysharp.Threading.Tasks;
using Game.Spells;
using UnityEngine;
using DG.Tweening;
namespace Game.Spells.Poison
{
    public class Poison : SpellBase
    {
        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/Poison");
            base.Initialize();
        }
        protected override void SetRange()
        {
            scaleAmount = 10f;
            base.SetRange();
        }
        protected override async UniTaskVoid Spell()
        {
            var scaleDuration = 0.5f;
            particle.Play();
            var scale = new Vector3(scaleAmount, 0.01f, scaleAmount);
            var tween = gameObject.Scaler(new Vector3TweenSetup(scale, scaleDuration,Ease.Linear));
            var time = 0f;
            var damageInterval = 0f;
            var isFirstHit = false;
            while(time <spellDuration)
            {
                if (!isFirstHit) { spellEffectHelper.EffectToUnit(); isFirstHit = true; }
                time += Time.deltaTime;
                damageInterval += Time.deltaTime;
                if(damageInterval >= 1.0f)
                {
                    spellEffectHelper.EffectToUnit();
                    damageInterval = 0f;
                }

                await UniTask.Yield();
            }
            particle.gameObject.transform.SetParent(null);
            particle.gameObject.transform.localScale = Vector3.one;
            var duration = 1.0f;
            var endValue = Vector3.zero;
            var ease = Ease.Linear;
            Vector3TweenSetup tweenSetup = new Vector3TweenSetup(endValue,duration, ease);
            var task = gameObject.Scaler(tweenSetup).ToUniTask();
            DestroyAll();
        }

       
        protected override async void DestroyAll()
        {
            Destroy(gameObject);
            while (particle.IsAlive())
            {
                await UniTask.Yield();
            }
            Destroy(particle.gameObject);
        }
    }
}


