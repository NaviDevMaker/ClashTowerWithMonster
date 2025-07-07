using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public static class FadeProcessHelper
{
    public static async UniTask FadeOutColor(float fadeDuration, CancellationToken cancellationToken, Material material)
    {
        Debug.Log("Player�̃t�F�C�h�A�E�g�J�n");
        var time = 0f;
        var meshMaterial = material;


        var startColor = meshMaterial.color;
        var startAlpha = startColor.a;

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
            Debug.Log("�F�ύX�L�����Z������܂���");
        }
        finally
        {
            var finalColor = startColor;
            finalColor.a = 0f;
            meshMaterial.color = finalColor;
            Debug.Log(meshMaterial.color.a);
        }
    }

    public static async UniTask FadeInColor(float fadeDuration,CancellationToken cancellationToken,Material material)
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
            Debug.Log("�F�ύX�L�����Z������܂���");
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

        Debug.Log("�v���p�e�B�𔭌����܂���");
        // SurfaceType = Transparent �����̐ݒ�
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_Surface", 1); // 0:Opaque, 1:Transparent�iURP�p�j
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0); // ���߃I�u�W�F�N�g��ZWrite��؂�
    }
}
