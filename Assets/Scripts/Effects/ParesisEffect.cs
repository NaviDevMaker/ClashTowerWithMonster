using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ParesisEffect
{
    public ParesisEffect()
    {
        SetParesisEffect().Forget();
    }
    GameObject paresisEffect;

    public async void GenerateParesisEffect(UnitBase target,int attackCount)
    {
        if (attackCount != 0) return;
        Renderer renderer = null;
        if(target is IMonster || target is IPlayer) renderer = target.MySkinnedMeshes[0];
        else if(target.GetType() == typeof(TowerControlller)) renderer = target.MyMeshes[0];
        if (renderer == null) return;
        var center = renderer.bounds.center;
        var size = renderer.bounds.size.magnitude;  
        var particleObj = UnityEngine.Object.Instantiate(paresisEffect,center,Quaternion.identity);
        var originalScale = particleObj.transform.localScale;
        particleObj.transform.localScale = originalScale * size;
        particleObj.transform.SetParent(target.transform);
        var particle = particleObj.GetComponent<ParticleSystem>();
        particle.Play();

        try
        {
            await UniTask.WaitUntil(() => !target.statusCondition.Paresis.isActive, cancellationToken: target.GetCancellationTokenOnDestroy());
        }
        catch (OperationCanceledException)
        {
            Debug.Log("ターゲットが死んだのでキャンセルされました");
        }

        if(particle != null)
        {
            particle.Stop();
            UnityEngine.Object.Destroy(particleObj);
        }   
    }
    async UniTask SetParesisEffect()
    {
        paresisEffect = await SetFieldFromAssets.SetField<GameObject>("Effects/ParesisEffect");
    }
}
