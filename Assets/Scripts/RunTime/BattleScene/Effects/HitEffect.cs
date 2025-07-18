using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class HitEffect:IEffectSetter
{
    public HitEffect()
    {
        SetEffect();
    }

    GameObject hitEffect;
    public async void GenerateHitEffect(UnitBase target)
    {
        Debug.Log(target);
        var renderer = target.BodyMesh;
        //var body = 0;
        //if(target is IMonster || target is IPlayer) renderer = target.MySkinnedMeshes[body];
        //else if(target is TowerControlller) renderer = target.MyMeshes[body];

        if (renderer == null) return;
        var center = renderer.bounds.center;
        var meshSize = renderer.bounds.size;
        var pos = new Vector3(center.x,center.y,center.z);//* target.myScale.x,* target.myScale, * target.myScale.z
        var size = new Vector3(meshSize.x * target.myScale.x,meshSize.y * target.myScale.y,meshSize.z * target.myScale.z);
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
    public async void SetEffect()
    {
       hitEffect =  await SetFieldFromAssets.SetField<GameObject>("Effects/HitEffect");
    }
}
