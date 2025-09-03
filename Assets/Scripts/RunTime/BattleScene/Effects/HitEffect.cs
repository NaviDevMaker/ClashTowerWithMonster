using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using static UnityEngine.GraphicsBuffer;

public class HitEffect:IEffectSetter
{
    public HitEffect()
    {
        SetEffect();
    }

    GameObject hitEffect;
    GameObject mageAttackHitEffect;
    public async void GenerateHitEffect(UnitBase target)
    {
        Debug.Log(target);
        var renderer = target.BodyMesh;
       
        if (renderer == null) return;
        var center = renderer.bounds.center;
        var meshSize = renderer.bounds.size;
        var pos = new Vector3(center.x,center.y,center.z);
        var scale = target is TowerController ? Vector3.one : target.myScale;
        var size = new Vector3(meshSize.x * scale.x, meshSize.y * scale.y,meshSize.z * scale.z);
        var particleObj = UnityEngine.Object.Instantiate(hitEffect, pos, Quaternion.identity);
        var particle = particleObj.GetComponent<ParticleSystem>();
        var startSize = particle.main.startSize;
        startSize.mode = ParticleSystemCurveMode.TwoCurves;
        startSize.constantMin = size.x;
        startSize.constantMax = size.x * 2.0f;
        particleObj.transform.localScale = size;
        particleObj.transform.SetParent(target.transform);
        var destroyDuration = 0.25f;
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(destroyDuration), cancellationToken: target.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException) { return; }
        UnityEngine.Object.Destroy(particleObj);        
    }
    public async UniTask GenerateMageAttackHitEffect(Vector3 position, UnitBase target)
    {
        var particleObj = UnityEngine.Object.Instantiate(mageAttackHitEffect, position, Quaternion.identity);
        var particle = particleObj.GetComponent<ParticleSystem>();
        particle.Play();
        particleObj.transform.SetParent(target?.transform);
        try
        {
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            await task;
        }
        catch (OperationCanceledException) {}
        finally
        {
           if(particleObj != null) UnityEngine.Object.Destroy(particleObj);
        }
    }
    public async void SetEffect()
    {
       hitEffect =  await SetFieldFromAssets.SetField<GameObject>("Effects/HitEffect");
       mageAttackHitEffect = await SetFieldFromAssets.SetField<GameObject>("Effects/MageHitAttackEffect");
    }
}
