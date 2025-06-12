using System;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine.Rendering.PostProcessing;

public class MagicCircleEffect
{

    public MagicCircleEffect()
    {
        SetSummonParticle().Forget();
    }

    GameObject summonParticle;
   public GameObject summonPointerParticle { get; private set;}
    
    public async UniTask PointerSummonEffect(GameObject summonPointerEffect,CancellationToken cancellationToken)
    {
        var rotateSpeed = 40.0f;
        var rotateAmount = 0f;
        var direction = 1;
        while(!cancellationToken.IsCancellationRequested)
        {
            var perFrameRotate = (rotateSpeed * Time.deltaTime) * direction;
            summonPointerEffect.transform.Rotate(0f, perFrameRotate, 0f);
            rotateAmount += perFrameRotate;
            if(rotateAmount > 360f)
            {
                direction *= -1;
                rotateAmount = 0f;
            }

            await UniTask.Yield(cancellationToken:cancellationToken);
        }
    }
    public IEnumerator SummonEffect(Vector3 particlePos,CardType cardType)
    {
        var particleObj = UnityEngine.Object.Instantiate(summonParticle, particlePos, Quaternion.identity);
        var particle = particleObj.GetComponentInChildren<ParticleSystem>();

        if(cardType == CardType.Monster) particle.Play();
        var duration = particle.main.duration;

        var baseParticleObj = particleObj.transform.GetChild(1).gameObject;

        if(cardType == CardType.Spell)
        {
            var hexColor = "#6A5ACD";
            Color newColor;
            ColorUtility.TryParseHtmlString(hexColor,out newColor);
            var childParticle = baseParticleObj.GetComponent<ParticleSystem>();
            if(childParticle != null)
            {
                var main = childParticle.main;
                main.startColor = newColor;
            }
        }
        var originalScale = baseParticleObj.transform.localScale;
        UnityEngine.Object.Destroy(particleObj, duration);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(baseParticleObj.transform.DOScale(originalScale * 0.5f, duration / 4)
            .SetEase(Ease.Linear))
            .Append(baseParticleObj.transform.DOScale(originalScale * 2.0f, duration / 4));
        yield return null;
    }

    async UniTask SetSummonParticle()
    {
        summonParticle = await SetFieldFromAssets.SetField<GameObject>("Effects/SummonEffect");
        summonPointerParticle = await SetFieldFromAssets.SetField<GameObject>("Effects/BaseMagic");
    }

}
