using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Game.Monsters;
using System.Linq;
public static class FadeProcessHelper
{
    public static async UniTask FadeOutColor(float fadeDuration, Material material,CancellationToken cancellationToken = default)
    {
        Debug.Log("Playerのフェイドアウト開始");
        var time = 0f;
        var meshMaterial = material;


        var startColor = meshMaterial.color;
        var startAlpha = 1.0f;//startColor.a

        try
        {
            while (time <= fadeDuration && !cancellationToken.IsCancellationRequested)
            {
                var lerpedTime = time / fadeDuration;
                var color = startColor;
                color.a = Mathf.Lerp(startAlpha, 0f, lerpedTime);

                meshMaterial.color = color;
                //Debug.Log(meshMaterial.color.a);
                time += Time.deltaTime;

                await UniTask.Yield(cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("色変更キャンセルされました");
            return;
        }
        finally
        {
            var finalColor = startColor;
            finalColor.a = 0f;
            meshMaterial.color = finalColor;
            Debug.Log(meshMaterial.color.a);
        }
    }
    public static async UniTask FadeInColor(float fadeDuration, Material material,CancellationToken cancellationToken = default)
    {
        var time = 0f;
        var meshMaterial = material;


        var startColor = meshMaterial.color;
        var startAlpha = 0f;
        try
        {
            while (time <= fadeDuration && !cancellationToken.IsCancellationRequested)
            {
                var lerpedTime = time / fadeDuration;
                var color = startColor;
                color.a = Mathf.Lerp(startAlpha,1.0f,lerpedTime);

                meshMaterial.color = color;
                //Debug.Log(meshMaterial.color.a);
                time += Time.deltaTime;
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("色変更キャンセルされました");
        }
        finally
        {
            var finalColor = startColor;
            finalColor.a = 1.0f;
            meshMaterial.color = finalColor;
            Debug.Log(meshMaterial.color.a);
        }
    }
    public static async UniTask FadeOutText(Text text,float fadeDuration)
    {
        var tween = text.DOFade(0f, fadeDuration);
        var task = tween.ToUniTask();
        await task;
    }
    public static async UniTask FadeInText(Text text, float fadeDuration)
    {
        var tween = text.DOFade(1f, fadeDuration);
        var task = tween.ToUniTask();
        await task;
    }
    public static void ChangeToTranparent(Material material)
    {

        Debug.Log("プロパティを発見しました");
        // SurfaceType = Transparent 相当の設定
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_Surface", 1); // 0:Opaque, 1:Transparent（URP用）
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0); // 透過オブジェクトはZWriteを切る
    }

    public static void ChangeToOpaque(Material material)
    {
        material.SetOverrideTag("RenderType", "Opaque");
        material.SetInt("_Surface", 0);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        material.EnableKeyword("_SURFACE_TYPE_OPAQUE");
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetFloat("SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
        material.SetFloat("_ZWrite", 1);
    }
    public static async UniTask WaitFOAllMesh<Towner>(this Towner controller,float duration) where Towner : MonsterControllerBase<Towner>
    {
        var tasks = controller.meshMaterials.SelectMany(materials => materials.Select(material =>
        {
            ChangeToTranparent(material);
            return FadeOutColor(duration, material, controller.GetCancellationTokenOnDestroy());
        })).ToArray();
        await UniTask.WhenAll(tasks);
    }
    public static async UniTask WaitFIAllMesh<Towner>(this Towner controller, float duration) where Towner : MonsterControllerBase<Towner>
    {
        var tasks = controller.meshMaterials.SelectMany(materials => materials.Select(material =>
        {
            ChangeToTranparent(material);
            return FadeInColor(duration, material, controller.GetCancellationTokenOnDestroy());
        })).ToArray();
        await UniTask.WhenAll(tasks);
    }
}
