using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

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
}
