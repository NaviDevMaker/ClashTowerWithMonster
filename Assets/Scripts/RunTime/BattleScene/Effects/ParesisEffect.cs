using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class ParesisEffect:IEffectSetter
{
    public ParesisEffect()
    {
        SetEffect();
    }
    GameObject paresisEffect;

    public async void GenerateParesisEffect(UnitBase target,float interval)
    {
        var visualTokens = target.statusCondition.visualTokens;
        if (visualTokens.TryGetValue(StatusConditionType.Paresis,out var cls))
        {
            if (cls != null)
            {
                cls.Cancel();
                cls.Dispose();
            }
        }
        var newCls = new CancellationTokenSource();
        visualTokens[StatusConditionType.Paresis] = newCls;
        var renderer = target.BodyMesh;
        //if(target is IMonster || target is IPlayer) renderer = target.MySkinnedMeshes[0];
        //else if(target.GetType() == typeof(TowerControlller)) renderer = target.MyMeshes[0];
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
            var visualCls = visualTokens[StatusConditionType.Paresis];
            var doubleCls = CancellationTokenSource.CreateLinkedTokenSource(visualCls.Token, target.GetCancellationTokenOnDestroy());
            await UniTask.Delay(TimeSpan.FromSeconds(interval),cancellationToken: doubleCls.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("ターゲットが死んだor効果が切れたor新たに掛けられたためキャンセルします");
        }
        finally
        {
            if (particle != null)
            {
                particle.Stop();
                UnityEngine.Object.Destroy(particleObj);
            }
        }   
    }
    public async void SetEffect()
    {
        paresisEffect = await SetFieldFromAssets.SetField<GameObject>("Effects/ParesisEffect");
    }
}
