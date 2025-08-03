using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public static class MaterialPropertySetter
{
    public static async UniTask SetLerpValue(this Material material,float start,float end,
        float duration,string propertyName,CancellationTokenSource cls = null)
    {
        if(!material.HasProperty(propertyName))
        {
            Debug.LogWarning("This property don't exist!!");
        }
        var time = 0f;

        try
        {
            while (time < duration && !cls.IsCancellationRequested)
            {
                time += Time.deltaTime;
                var lerp = time / duration;
                var value = Mathf.Clamp01(Mathf.Lerp(start, end, lerp));
                material.SetFloat(propertyName, value);
                await UniTask.Yield(cancellationToken: cls.Token);
            }
        }
        catch (OperationCanceledException) { return; }
    }
}
