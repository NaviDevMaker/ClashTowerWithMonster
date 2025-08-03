using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public struct Vector3TweenSetup
{ 
    public float duration { get; set; }
    public Vector3 endValue { get; set; }
    public Ease ease { get; set; }

    public Vector3TweenSetup(Vector3 endvalue,float duration, Ease ease = Ease.OutQuad)
    {
        this.duration = duration;
        this.endValue = endvalue;
        this.ease = ease;
    }
}

public struct Vector2TweenSetup
{
    public float duration { get; set; }
    public Vector2 endValue { get; set; }
    public Ease ease { get; set; }

    public Vector2TweenSetup(Vector2 endvalue, float duration, Ease ease = Ease.OutQuad)
    {
        this.duration = duration;
        this.endValue = endvalue;
        this.ease = ease;
    }
}

public struct FadeSet
{ 
    public float alpha { get; set; }
    public float duration { get; set; }

    public Ease ease { get; set; }

    public FadeSet(float alpha,float duration,Ease ease = Ease.InOutQuad)
    {
        this.alpha = alpha;
        this.duration = duration;
        this.ease = ease;
    }
}

public static class ObjectTweenFunctions
{
   public static Tween Scaler(this GameObject origin,Vector3TweenSetup tweenSetup)
   {
      var duration = tweenSetup.duration;
      var endValue = tweenSetup.endValue;
      var ease = tweenSetup.ease;
      var tween = origin.transform.DOScale(endValue,duration).SetEase(ease);
      return tween;
   }
   public static Tween Mover(this GameObject origin,Vector3TweenSetup tweenSetup)
   {
        var duration = tweenSetup.duration;
        var endValue = tweenSetup.endValue;
        var ease = tweenSetup.ease;
        var tween = origin.transform.DOMove(endValue, duration).SetEase(ease);
        return tween;
   }

    public static Tween RectMover(this Graphic origin,Vector2TweenSetup tweenSetup)
    {
        var duration = tweenSetup.duration;
        var endValue = tweenSetup.endValue;
        var ease = tweenSetup.ease;
        var tween = origin.rectTransform.DOAnchorPos(endValue, duration).SetEase(ease);
        return tween;
    }
    public static Tween Roter(this GameObject origin, Vector3TweenSetup tweenSetup)
    {
        var duration = tweenSetup.duration;
        var endValue = tweenSetup.endValue;
        var ease = tweenSetup.ease;
        var tween = origin.transform.DORotate(endValue, duration).SetEase(ease);
        return tween;
    }

    public static Tween Fader(this Graphic graphic,FadeSet fadeSet)
    {
        Debug.Log("FadeŠJŽn‚µ‚Ü‚µ‚½");
        var duration = fadeSet.duration;
        var alpha = fadeSet.alpha;
        var ease = fadeSet.ease;
        var tween = graphic.DOFade(alpha,duration).SetEase(ease);
        return tween;
    }

}
