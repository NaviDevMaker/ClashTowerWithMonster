using UnityEngine;

public class Test2 : MonoBehaviour
{
    // Start is called once before the firstColor execution of Update after the MonoBehaviour is created
    void Start()
    {
        IColliderRangeProvider colliderRangeProvider = null;

        if (TryGetComponent<BoxCollider>(out var boxCollider))
        {
            Debug.Log("ボックスコライダーからセットします");
            colliderRangeProvider = new BoxColliderrangeProvider { boxCollider = boxCollider };
            var rangeX = colliderRangeProvider.GetRangeX();
            var rangeZ = colliderRangeProvider.GetRangeZ();
            var prioritizedRange = colliderRangeProvider.GetPriorizedRange();
            var timerOffsetY = colliderRangeProvider.GetTimerOffsetY();
            Debug.Log($"テストです、{rangeX}{rangeZ}{prioritizedRange}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
