using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ConfusionEffect : IEffectSetter
{
    public ConfusionEffect() => SetEffect();

    GameObject confusionHitEffect;
    GameObject confusionOverheadEffect;

    public async UniTask<ParticleSystem> GenerateConfusionEffect(UnitBase target, float duration,CancellationTokenSource cls)
    {
        var collider = target.GetComponent<Collider>();
        var y = collider.bounds.max.y;
        var offsetY = 0.5f;
        var height = y + offsetY;
        var pos = target.transform.position;
        pos.y += height;
        var rot = confusionOverheadEffect.transform.rotation;
        var particleObj = UnityEngine.Object.Instantiate(confusionOverheadEffect, pos, rot);
        particleObj.transform.SetParent(target.transform);
        var uniscale = target.UnitScale;
        var parentP = particleObj.GetComponent<ParticleSystem>();
        var parentCriterionScale = 2f;
        ParticleModuleSet(uniscale, parentP,parentCriterionScale);
        var childparticles = particleObj.GetComponentsInChildren<ParticleSystem>();

        childparticles.ToList().ForEach(p =>
        {
            var criterion = 2.5f;
            var magnitude = target.transform.lossyScale.magnitude;
            var originalScale  = p.transform.lossyScale / magnitude;
            var scale = GetScale(uniscale, originalScale, criterion);
            p.gameObject.transform.localScale = scale;
        });    

        var cmp = particleObj.GetComponent<ParticleSystem>();
        var main = cmp.main;
        main.loop = true;
        cmp.Play();
        try
        {
            await UniTask.WhenAll(UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: target.GetCancellationTokenOnDestroy()),
                UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cls.Token));
        }
        catch (OperationCanceledException)
        {
            if (target != null)
            {
                main.loop = false;
                UnityEngine.Object.Destroy(particleObj);
                return null;
            }        
        }
      
        main.loop = false;
        return cmp;
    }
    public async UniTask GenerateConfusionHitEffect(UnitBase target)
    {
        var y = target.BodyMesh.bounds.size.y;
        var pos = target.transform.position;
        pos.y += y;
        var rot = confusionHitEffect.transform.rotation;
        var unitScale = target.UnitScale;
      
        var particleObj = UnityEngine.Object.Instantiate(confusionHitEffect,pos, rot);
        var magnitude = target.transform.lossyScale.magnitude;
        var originalScale = particleObj.transform.localScale /  magnitude;
        var criterionScale = 2f;
        var scale = GetScale(unitScale, originalScale, criterionScale);
        if (particleObj == null) return;
        particleObj.transform.localScale = scale;
        var cmp = particleObj.GetComponent<ParticleSystem>();
        var duration = 0.5f;
        cmp.Play();
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        UnityEngine.Object.Destroy(particleObj);
    }

    void  ParticleModuleSet(UnitScale unitScale,ParticleSystem particle,float criterionAmount)
    {
        if (particle == null) return;
        var main = particle.main;
        var scaleAmount = unitScale switch
        {
            UnitScale.player or UnitScale.small => criterionAmount,
            UnitScale.middle => criterionAmount * 1.2f,
            UnitScale.large => criterionAmount * 1.5f,
            _ => default
        };
        var startSize = main.startSize;
        startSize.constantMin *= scaleAmount;
        startSize.constantMax *= scaleAmount;
        main.startSize = startSize;
    }

    Vector3 GetScale(UnitScale unitScale,Vector3 originalScale,float criterionAmount)
    {
        var scale = unitScale switch
        {
            UnitScale.small or UnitScale.player => originalScale,
            UnitScale.middle => originalScale * criterionAmount,
            UnitScale.large => originalScale * criterionAmount * 1.5f,
            _ => default
        };

        return scale;
    }
    public async void SetEffect()
    {
        confusionHitEffect  = await SetFieldFromAssets.SetField<GameObject>("Effects/ConfusionHitEffect");
        confusionOverheadEffect = await SetFieldFromAssets.SetField<GameObject>("Effects/ConfusionOverheadEffect");
    }
}
