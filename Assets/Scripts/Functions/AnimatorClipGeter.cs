using UnityEngine;

public static class AnimatorClipGeter
{
    public static AnimationClip GetAnimationClip(Animator animator,string wantClipName)
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

}
