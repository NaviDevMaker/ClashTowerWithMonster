using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UIFuctions
{
    public static Tween ShakeText(Graphic UiCompoent, float duration = 0.1f,float strength = 50f,int vibrato = 30)
    {
        //var duration = 0.1f;
        //var strength = 50f;
        //var vibrato = 30;
       var tween = UiCompoent.rectTransform.DOShakeAnchorPos(duration, strength, vibrato)
            .SetEase(Ease.Linear)
            .SetLoops(1, LoopType.Yoyo);
        return tween;
    }
    public static void LookToCamera(GameObject rotateObject)
    {
        var camera = Camera.main.gameObject;
        var direction = camera.transform.position - rotateObject.transform.position;

        var projected = Vector3.ProjectOnPlane(direction, rotateObject.transform.right);
        Quaternion rot = Quaternion.LookRotation(projected, rotateObject.transform.up);
        rotateObject.transform.rotation = rot;
    }
}
