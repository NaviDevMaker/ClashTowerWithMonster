using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class ExplosionEffect
{
    public ExplosionEffect() => SetExplosionEffect();

    GameObject explosionEffect;
    public async void GenerateExplosionEffect(Vector3 pos)
    {
        var rot = explosionEffect.transform.rotation;
        var particleObj = UnityEngine.Object.Instantiate(explosionEffect, pos, rot);
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
    async void SetExplosionEffect()
    {
        explosionEffect = await SetFieldFromAssets.SetField<GameObject>("Effects/ExplosionEffect");
    }
}
