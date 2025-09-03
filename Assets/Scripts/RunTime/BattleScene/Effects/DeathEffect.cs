using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem.DualShock.LowLevel;
using Game.Monsters.Archer;
using Unity.VisualScripting;

public class DeathEffect:IEffectSetter
{
   public DeathEffect()
   {
        SetEffect();
   }
   GameObject deathParticle;

    public async void GenerateDeathEffect<T>(T unit,float animationLength) where T : MonoBehaviour
   {
        var unitPos = unit.gameObject.transform.position;
        Renderer meshRenderer = default;
        //var body = 0;
        if (unit is IMonster || unit is IPlayer || unit is TowerController)
        {
           var unitBase = unit as UnitBase;
           meshRenderer =  unitBase.BodyMesh;
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
        if (duration <= 0f || float.IsNaN(duration) || float.IsInfinity(duration))
        {
            Debug.LogWarning("Particle duration is zero or invalid. Skipping delay adjustment.");
            duration = animationLength + delay;
        }
        else
        {
            var adjust = animationLength / duration;

            if(!float.IsNaN(adjust) && !float.IsInfinity(adjust))
            {
                duration = duration * adjust + delay;
            }
            else duration = animationLength + delay;
        }
        //var adjust = animationLength / duration;
        //duration = duration * adjust + delay;
        particle.Play();
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        UnityEngine.Object.Destroy(particleObj);
   }

   public async void SetEffect()
   {
        deathParticle = await SetFieldFromAssets.SetField<GameObject>("Effects/DeathEffect");
   }
}
