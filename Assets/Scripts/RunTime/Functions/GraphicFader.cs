using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class GraphicFader
{
    public static async UniTask FadeOut(this Graphic origin, float startValue, float endValue, float duration,CancellationTokenSource cls)
    {
        var time = 0f;
        var newColor = origin.color;

        while (time < duration && !cls.IsCancellationRequested)
        {
                time += Time.deltaTime;
                var lerp = time / duration;
                var value = Mathf.Clamp01(Mathf.Lerp(startValue, endValue, lerp));
                newColor.a = value;
                origin.color = newColor;
                await UniTask.Yield(cancellationToken: cls.Token);
        }
       
    }
}
