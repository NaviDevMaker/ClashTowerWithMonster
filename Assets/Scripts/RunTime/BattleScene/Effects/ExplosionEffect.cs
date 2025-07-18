using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class ExplosionEffect:IEffectSetter
{
    public ExplosionEffect() => SetEffect();

    GameObject explosionEffect;
    public async void GenerateExplosionEffect(Vector3 pos,float scaleAmount = 1)
    {
        var rot = explosionEffect.transform.rotation;
        var scale = explosionEffect.transform.localScale * scaleAmount;
        var particleObj = UnityEngine.Object.Instantiate(explosionEffect, pos, rot);
        particleObj.transform.localScale = scale;
        var particleList = particleObj.GetComponentsInChildren<ParticleSystem>().ToList();
        var particle = particleObj.GetComponent<ParticleSystem>();
        particle.Play();
        var tasks = new List<UniTask>();
        particleList.ForEach(p =>
        {
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
            tasks.Add(task);
        });
        await UniTask.WhenAll(tasks);
        UnityEngine.Object.Destroy(particleObj);
    }
    public async void SetEffect()
    {
        explosionEffect = await SetFieldFromAssets.SetField<GameObject>("Effects/ExplosionEffect");
    }
}
