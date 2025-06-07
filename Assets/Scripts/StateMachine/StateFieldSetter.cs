using Game.Monsters;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class StateFieldSetter
{
    public static void AttackStateFieldSet<T>(MonsterControllerBase<T> controller,AttackStateBase<T> attackState,
        float clipLength,int attackEndFrame,float interval) where T : MonsterControllerBase<T>
    {
        var frameRate = AnimatorClipGeter.GetAnimationClip(controller.animator, controller.MonsterAnimPar.attackAnimClipName).frameRate;
        attackState.maxFrame = Mathf.RoundToInt(frameRate * clipLength);
        attackState.attackEndFrame = attackEndFrame;
        attackState.attackEndNomTime = (float)attackEndFrame / (float)attackState.maxFrame;
        attackState.interval = interval;
    }  
}
