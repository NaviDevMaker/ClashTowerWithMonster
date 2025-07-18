using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ToxicSmokeEffect:IEffectSetter
{
    public ToxicSmokeEffect() => SetEffect();

    GameObject toxicEfffect;

    public async void ToxicEffectSet(UnitBase unit,float duration)
    {
        var pos = unit.transform.position;
        var scale = unit.myScale.sqrMagnitude;
        var toxicSmoke = UnityEngine.Object.Instantiate(toxicEfffect,pos,toxicEfffect.transform.rotation);
        toxicSmoke.transform.localScale *= scale;
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        UnityEngine.Object.Destroy(toxicSmoke);
    }
    public async void SetEffect()
    {
        toxicEfffect = await SetFieldFromAssets.SetField<GameObject>("Effects/Poison-Smoke");
    }

}
