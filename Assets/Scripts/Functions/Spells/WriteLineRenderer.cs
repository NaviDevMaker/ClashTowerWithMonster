using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public static class WriteLineRenderer
{
    public static void SetUpLineRenderer(this LineRenderer lineRenderer)
    {
        lineRenderer.numCapVertices = 0;
        lineRenderer.numCornerVertices = 0;
        lineRenderer.widthMultiplier = 0.25f;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public static void DrawRange(this LineRenderer lineRenderer, Vector3 center, float radiusX, float radiusZ
        , float offsetY = 0f)
    {
        if (!lineRenderer.enabled)
        {
            lineRenderer.enabled = true;
        }
       
        var segument = 100;
        lineRenderer.positionCount = segument;
        lineRenderer.loop = true;
        for (int i = 0; i < segument; i++)
        {
            var angle = ((float)i / segument) * Mathf.PI * 2;
            var x = Mathf.Cos(angle) * radiusX;
            var z = Mathf.Sin(angle) * radiusZ;
            var nextPos = new Vector3(x, 0, z) + center;
            nextPos.y = Terrain.activeTerrain.SampleHeight(nextPos) + offsetY;
            lineRenderer.SetPosition(i, nextPos);
        }
    }

    public static async UniTask ShurinkRangeLine(this LineRenderer lineRenderer,Vector3 center,float radiusX,float radiusZ)
    {
        var duration = 0.3f;
        var time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            var lerpedTime = Mathf.Clamp01(time / duration);
            var currentRadiusX = Mathf.Lerp(radiusX, 0f, lerpedTime);
            var currentRadiusZ = Mathf.Lerp(radiusZ, 0f, lerpedTime);
            DrawRange(lineRenderer,center,currentRadiusX, currentRadiusZ);
            await UniTask.Yield();
        }

        lineRenderer.enabled = false;
    }
    public static async void LitLineRendererMaterial(this LineRenderer lineRenderer,Func<float,float,Material,UniTask> waitAction)
    {
        var material = lineRenderer.material;
        material.EnableKeyword("_EMISSION");
        var radAdjust = 2.0f;
        var maxIntencity = 1.5f;

        try
        {
            await waitAction(radAdjust, maxIntencity, material);
        }
        catch (OperationCanceledException) { }
    }
}
