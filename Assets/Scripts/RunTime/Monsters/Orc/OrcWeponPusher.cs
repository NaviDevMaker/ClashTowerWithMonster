using Cysharp.Threading.Tasks;
using Game.Monsters.Orc;
using System;
using UnityEngine;

public class OrcWeponPusher : PushableWeponBase<OrcController>
{
    float pushAmount = 0f;
    float perPushDuration = 0f;
    public override void Initialize(OrcController owner)
    {
        moveType = owner.moveType;
        UnitScale = owner.UnitScale;
        var rangeInfo = owner.RangeAttackMonsterStatusData._RangeAttackInfo;
        pushAmount = rangeInfo.PushAmount;
        perPushDuration = rangeInfo.PerPushDuration;
        base.Initialize(owner);
    }
    public async void PushUnitToRight(UnitBase target)
    {
        var isEffectiveScale = UnitScale.PlayerSmallMiddle;
        if ((isEffectiveScale & target.UnitScale) == 0) return;
        Debug.Log("—LŒø‚Å‚·");
        try
        {
            target.isKnockBacked_Spell = true;
            var direction = weponOwner.transform.right;          
            var push = direction * pushAmount;
            var targetPos = target.transform.position + push;
            var moveSet = new Vector3TweenSetup(targetPos,perPushDuration);
            await target.gameObject.Mover(moveSet).ToUniTask(cancellationToken:target.GetCancellationTokenOnDestroy());
            target.isKnockBacked_Spell = false;
        }
        catch (OperationCanceledException) { }
    }
}
