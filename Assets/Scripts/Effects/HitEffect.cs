using UnityEngine;
using Cysharp.Threading.Tasks;

public class HitEffect
{
    public HitEffect()
    {
        SetHitEffectParticle().Forget();
    }

    GameObject hitEffect;
    public void GenerateHitEffect(UnitBase target)
    {
        Renderer renderer = null;
        if(target is IMonster || target is IPlayer) renderer = target.MySkinnedMeshes[0];
        else if(target is TowerControlller) renderer = target.MyMeshes[0];

        if (renderer == null) return;
        var center = renderer.bounds.center;
        var meshSize = renderer.bounds.size;
        var pos = new Vector3(center.x * target.myScale.x, center.y * target.myScale.y,center.z * target.myScale.z);
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
        UnityEngine.Object.Destroy(particleObj, destroyDuration);
        
    }
    async UniTask SetHitEffectParticle()
    {
       hitEffect =  await SetFieldFromAssets.SetField<GameObject>("Effects/HitEffect");
    }
}
