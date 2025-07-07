using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ToxicSmokeEffect
{
    public ToxicSmokeEffect() => SetToxicParticle();

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
    async void SetToxicParticle()
    {
        toxicEfffect = await SetFieldFromAssets.SetField<GameObject>("Effects/Poison-Smoke");
    }

}
