using Cysharp.Threading.Tasks;
using Game.Spells;
using UnityEngine;
using DG.Tweening;
namespace Game.Spells.Poison
{
    public class Poison : SpellBase
    {
        protected override void SetRange()
        {
            scaleAmount = 10f;
            base.SetRange();
        }
        protected override async UniTaskVoid Spell()
        {
            var scaleDuration = 0.5f;
            particle.Play();
            transform.DOScale(Vector3.one * scaleAmount, scaleDuration);
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
            StartScaleToZero();
        }

        async void StartScaleToZero()
        {
            var duration = 1.0f;
            var tween = transform.DOScale(Vector3.zero, duration);
            var task = tween.ToUniTask();
            await task;
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


