using UnityEngine;

public static class TargetPositionGetter
{
    public static float GetTargetHeight(UnitBase target)
    {
        Debug.Log(target);
        var body = 0;
        Renderer meshRenderer = null;
        if (target.MySkinnedMeshes.Count != 0) meshRenderer = target.MySkinnedMeshes[body];
        else if (target.MyMeshes.Count != 0) meshRenderer = target.MyMeshes[body];
        if (meshRenderer != null) return meshRenderer.bounds.size.y / 2;
        else return 1f;
    }
}
