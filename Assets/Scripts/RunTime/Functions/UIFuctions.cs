using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class UIFuctions
{
    public static Tween ShakeUI(Graphic UiCompoent, float duration = 0.1f,float strength = 50f,int vibrato = 30)
    {
        //var duration = 0.1f;
        //var strength = 50f;
        //var vibrato = 30;
       var originalPos = UiCompoent.rectTransform.anchoredPosition;
       var tween = UiCompoent.rectTransform.DOShakeAnchorPos(duration, strength, vibrato)
             .SetEase(Ease.Linear)
             .SetLoops(1, LoopType.Yoyo)
             .OnComplete(() => UiCompoent.rectTransform.anchoredPosition = originalPos);
            return tween;
    }

    public static Tween ScaleUI(Graphic UIComponent,float duration = 0.05f,float scaleAmount = 1.3f)
    {
        var originalScale = UIComponent.rectTransform.localScale;
        var targetScale = originalScale * scaleAmount;
        var sequence = DOTween.Sequence();
        sequence.Append(UIComponent.rectTransform.DOScale(targetScale, duration))
            .SetEase(Ease.Linear)
            .Append(UIComponent.rectTransform.DOScale(originalScale,duration))
            .SetEase(Ease.Linear);
        return sequence;
    }

    public static Tween SlideUI(Graphic UIComponent,float duration,float endValue,float delay = 0f)
    {
        var startPosX = UIComponent.rectTransform.anchoredPosition.x;
        var sequence = DOTween.Sequence();
        sequence.Append(UIComponent.rectTransform.DOAnchorPosX(endValue, duration))
            .AppendInterval(delay)
            .Append(UIComponent.rectTransform.DOAnchorPosX(startPosX, duration));
        return sequence;
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
