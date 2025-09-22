using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class AnimatorMethod
{
    public static AnimationClip GetAnimationClip(this Animator animator,string wantClipName)
    { 
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        var clips = controller.animationClips;
        foreach (var clip in clips)
        {
            Debug.Log(clip.name);
            if (clip.name == wantClipName) return clip;
        }

        Debug.LogError("指定されたアニメーションのクリップはこのアニメーターに存在しません");
        return null;
    }
    public static float GetRepeatInterval(this Animator animator,float startNormalizeTime, int repeatCount,
                                          float clipLength,float stateAnimSpeed)
    {
        var now = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        var elapsedNormalizedTime = now - startNormalizeTime;
        var startLength = elapsedNormalizedTime * clipLength;
        var leftLength = clipLength - startLength;
        var repeatInterval = leftLength / (float)repeatCount;
        return (repeatInterval / stateAnimSpeed) / animator.speed;
    }
    public static float GetCurrentNormalizedTime(this Animator animator,float startNormalizeTime)
    {
        var current = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        return current - startNormalizeTime;
    }

    public static void ChangeClipForAnimationEvent(this Animator animator,AnimatorOverrideController overrideController,
                                                   string clipName,float clipLength)
    {
        var attackMotionClip = animator.GetAnimationClip(clipName);
        var eventSetTime = clipLength - 0.01f;

        var newClip = UnityEngine.Object.Instantiate(attackMotionClip);
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.functionName = "StopAnimation_AttackState";
        newClip.name = clipName + "_PlusEvent";
        animationEvent.time = eventSetTime;
        newClip.AddEvent(animationEvent);

        overrideController[attackMotionClip.name] = newClip;
    }

}
