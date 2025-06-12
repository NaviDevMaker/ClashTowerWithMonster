using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UIFuctions
{
    public static void ShakeText(Graphic UiCompoent)
    {
        var duration = 0.1f;
        var strength = 50f;
        var vibrato = 30;
        UiCompoent.rectTransform.DOShakeAnchorPos(duration, strength, vibrato)
            .SetEase(Ease.Linear)
            .SetLoops(1, LoopType.Yoyo);
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
