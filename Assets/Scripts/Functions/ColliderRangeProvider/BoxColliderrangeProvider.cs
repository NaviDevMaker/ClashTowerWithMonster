using UnityEngine;

public class BoxColliderrangeProvider : IColliderRangeProvider
{
    public BoxCollider boxCollider;
    public float GetPriorizedRange(){ return Mathf.Max(GetRangeX(), GetRangeZ()); }
    public float GetRangeX() { return boxCollider.bounds.extents.x; }
    public float GetRangeZ() { return boxCollider.bounds.extents.z; }

    public float GetTimerOffsetY() { return boxCollider.bounds.extents.y;}
}
