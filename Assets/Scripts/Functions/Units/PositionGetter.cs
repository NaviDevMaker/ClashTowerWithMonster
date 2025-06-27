using UnityEngine;

public static class PositionGetter
{
    public static Vector3 GetPerTargetPos(Vector3 currentPos, Vector3 direction,float flyingOffsetY = 0)
    {
        var targetPos = currentPos + direction;
        targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos) + flyingOffsetY;
        return targetPos;
    }

    public static Vector3 GetFlatPos(Vector3 pos)
    {
        var flatTargetPosition = new Vector3(pos.x, 0f, pos.z);
        return flatTargetPosition;
    }

}
