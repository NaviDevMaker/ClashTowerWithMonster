using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
public class BuffEffect
{
    public BuffEffect()
    {
        SetEffects();
    }

    GameObject buffEffect_Power;
    GameObject buffEffect_Speed;
    GameObject buffAura_Power;
    GameObject buffAura_Speed;

    public async UniTask SetEffectToUnit(UnitBase origin,UnitBase target,BuffType buffType)
    {
        Renderer mesh = null;
        var body = 0;
        mesh = target.MySkinnedMeshes[body];
        if (mesh == null) return;
        await AuraMoveToTarget(origin,mesh,buffType);
        var approxiSize = target is IPlayer? mesh.bounds.size.magnitude * 2.0f:mesh.bounds.size.magnitude;
        var targetPos = target.transform.position;
        GameObject particleObj = null;
        if (buffType == BuffType.Power) particleObj = Object.Instantiate(buffEffect_Power,targetPos,buffEffect_Power.transform.rotation);
        else if (buffType == BuffType.Speed) particleObj = Object.Instantiate(buffEffect_Speed,targetPos, buffEffect_Speed.transform.rotation);
        else return;
        var originalScale = particleObj.transform.localScale;
        particleObj.transform.localScale = approxiSize * originalScale;
        particleObj.transform.SetParent(target.transform);
    }
    async UniTask AuraMoveToTarget(UnitBase origin,Renderer renderer,BuffType buffType)
    {
        Debug.Log("オーラエフェクト呼ばれたよ");
        var targetPos = renderer.bounds.center;
        var distance = (origin.transform.position - targetPos).magnitude;
        var moveSpeed = 10.0f;
        var body = 0;
        var originCenter = origin.MySkinnedMeshes[body].bounds.center;
        var pos = originCenter;
        GameObject particleObj = null;
        if (buffType == BuffType.Power) particleObj = Object.Instantiate(buffAura_Power, pos, buffAura_Power.transform.rotation);
        else if (buffType == BuffType.Speed) particleObj = Object.Instantiate(buffAura_Speed, pos, buffAura_Speed.transform.rotation);
     
        while(true)
        {
            if (renderer == null) break;
            targetPos = renderer.bounds.center;
            if ((targetPos - particleObj.transform.position).magnitude <= 0.2f) break;
            var move = Vector3.MoveTowards(particleObj.transform.position, targetPos, moveSpeed * Time.deltaTime);
            particleObj.transform.position = move;
            await UniTask.Yield();
        }
        Object.Destroy(particleObj);
    }
    async void SetEffects()
    {
        buffEffect_Power = await SetFieldFromAssets.SetField<GameObject>("Effects/Buff_PowerEffect");
        buffEffect_Speed = await SetFieldFromAssets.SetField<GameObject>("Effects/Buff_SpeedEffect");
        buffAura_Power = await SetFieldFromAssets.SetField<GameObject>("Effects/BuffAuraEffect _Speed");
        buffAura_Speed = await SetFieldFromAssets.SetField<GameObject>("Effects/BuffAuraEffect_Power");
    }
}

public enum BuffType
{ 
    Power,
    Speed,
}

