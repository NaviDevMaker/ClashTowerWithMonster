using UnityEngine;

public interface IColliderRangeProvider
{
    float GetRangeX();
    float GetRangeZ();

    float GetTimerOffsetY();
    float GetPriorizedRange();
}
