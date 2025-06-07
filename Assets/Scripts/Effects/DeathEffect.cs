using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem.DualShock.LowLevel;
using Game.Monsters.Archer;
using Unity.VisualScripting;

public class DeathEffect
{
   public DeathEffect()
   {
        SetDeathPartcle().Forget();
   }
   GameObject deathParticle;

    public async void GenerateDeathEffect<T>(T unit,float animationLength) where T : MonoBehaviour
   {
        var unitPos = unit.gameObject.transform.position;
        Renderer meshRenderer = default;
        if (unit is UnitBase)
        {
           var unitBase = unit as UnitBase;
           var body = 0;
           meshRenderer = unitBase.MySkinnedMeshes[body];
        }
        else if(unit.GetType() == typeof(ArcherController))
        {
            var archerController = unit as ArcherController;
            meshRenderer = archerController.MyMesh;
        }
        var particleScale = deathParticle.transform.localScale;
        if (meshRenderer != null)
        {
            var meshSize = meshRenderer.bounds.size;
            particleScale = new Vector3(particleScale.x * meshSize.x,
                particleScale.y * meshSize.y,particleScale.z * meshSize.z);
        }
        var particleObj = UnityEngine.Object.Instantiate(deathParticle, unitPos, Quaternion.identity);
        //particleObj.transform.SetParent(unit.transform);
        particleObj.transform.localScale = particleScale;
        var particle = particleObj.GetComponent<ParticleSystem>();
        var duration = particle.main.duration;
        var delay = 0.25f;//死んでから若干エフェクトを出し続けるため
        var adjust = animationLength / duration;
        duration = duration * adjust + delay;
        particle.Play();
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        UnityEngine.Object.Destroy(particleObj);
   }

   async UniTask SetDeathPartcle()
   {
        deathParticle = await SetFieldFromAssets.SetField<GameObject>("Effects/DeathEffect");
   }
}
