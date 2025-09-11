using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealEffect : IEffectSetter
{
    public HealEffect() => SetEffect();
    GameObject healEffectObj;

    public async void GenerateUnitHealEffect(UnitBase target)
    {
        var particles = new List<ParticleSystem>();
        try
        {
            var body = target.BodyMesh;
            if (body == null) return;
            var scale = body.bounds.size * 0.5f;
            var targetPos = PositionGetter.GetFlatPos(target.transform.position);
            if (target is IMonster monster)
            {
                var data = monster._MonsterStatus;
                if (data == null) return;
                if (data is IFlying flying)
                {
                    var offset = Vector3.up * flying.FlyingOffsetY;
                    targetPos += offset;
                }
            }
            var rot = healEffectObj.transform.rotation;
            var healObj = UnityEngine.Object.Instantiate(healEffectObj, targetPos, rot);
            var originalScale = healObj.transform.lossyScale;
            var targetScale = new Vector3(originalScale.x * scale.x, originalScale.y, originalScale.z * scale.z);
            healObj.transform.SetParent(target.transform);
            healObj.transform.localScale = targetScale;
            particles = healObj.GetComponentsInChildren<ParticleSystem>().ToList();
            var tasks = particles.Select(p =>
            {
                if (p == null) return UniTask.CompletedTask;
                return RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
            }).ToList();
            await tasks;
        }
        finally
        {
            particles.ForEach(p =>
            {
                if (p == null) return;
                UnityEngine.Object.Destroy(p.gameObject);
            });
        }
    }
    public async void SetEffect() => healEffectObj = await SetFieldFromAssets.SetField<GameObject>("Effects/UnitHealEffect");
}
