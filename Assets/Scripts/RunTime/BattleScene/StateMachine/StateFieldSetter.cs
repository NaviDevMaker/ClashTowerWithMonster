using Game.Monsters;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class StateFieldSetter
{
    public static void AttackStateFieldSet<T>(MonsterControllerBase<T> controller,AttackStateBase<T> attackState,
        float clipLength,int attackEndFrame,float interval = 0f,bool changeClipByScript = true) where T : MonsterControllerBase<T>
    {
        string targetClipName;
        if (changeClipByScript) targetClipName = controller.MonsterAnimPar.attackAnimClipName + "_PlusEvent";
        else targetClipName = controller.MonsterAnimPar.attackAnimClipName;

        var frameRate = AnimatorClipGeter.GetAnimationClip(controller.animator,targetClipName).frameRate;
        attackState.maxFrame = Mathf.RoundToInt(frameRate * clipLength);
        attackState.attackEndFrame = attackEndFrame;
        attackState.attackEndNomTime = (float)attackEndFrame / (float)attackState.maxFrame;
        attackState.interval = interval;
    }  
}
