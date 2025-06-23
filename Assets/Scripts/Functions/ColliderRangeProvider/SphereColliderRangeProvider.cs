using UnityEngine;

public class SphereColliderRangeProvider : IColliderRangeProvider
{
    public SphereCollider sphereCollider;
    public float GetPriorizedRange(){ return sphereCollider.radius; }
  
    public float GetRangeX() { return sphereCollider.radius; }
  
    public float GetRangeZ() { return sphereCollider.radius; }

    public float GetTimerOffsetY() { return sphereCollider.radius; }
}
