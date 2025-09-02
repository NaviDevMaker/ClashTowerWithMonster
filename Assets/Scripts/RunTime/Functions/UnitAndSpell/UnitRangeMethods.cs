using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class UnitRangeMethods
{
    public static  Func<List<UnitBase>> GetUnitInWeponRange<T>(this T controller) where T : UnitBase
    {
        if (!(controller is IRangeAttack rangeAttack))
        {
            Debug.LogError("インターフェイス継承してないよ俺、しっかりしてくれ...");
            return null;
        }
        var rangeAttackObj = rangeAttack.rangeAttackObj;
        var collider = rangeAttackObj.GetComponent<Collider>();
        var rangeX = 0f;
        var rangeZ = 0f;
        var prioritizedRange = 0f;
        IColliderRangeProvider colliderRangeProvider = null;
        if (collider != null)
        {
            if (collider.TryGetComponent<BoxCollider>(out var boxCollider))
            {
                colliderRangeProvider = new BoxColliderrangeProvider { boxCollider = boxCollider };
                rangeX = colliderRangeProvider.GetRangeX();
                rangeZ = colliderRangeProvider.GetRangeZ();
                prioritizedRange = colliderRangeProvider.GetPriorizedRange();
            }
            else if (collider.TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                var scaleAmount = rangeAttackObj.transform.lossyScale.magnitude;
                colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
                rangeX = colliderRangeProvider.GetRangeX() * scaleAmount;
                rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount;
                prioritizedRange = colliderRangeProvider.GetPriorizedRange() * scaleAmount;
            }
        }

        Func<List<UnitBase>> getUnitsInWeponRange = () =>
        {
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(rangeAttackObj, prioritizedRange);
            if (sortedArray.Length == 0) return new List<UnitBase>();
            List<UnitBase> filteredList = new List<UnitBase>();
            var myType = controller.moveType;
            var effectiveSide = myType switch
            {
                MoveType.Walk => MoveType.Walk,
                MoveType.Fly => MoveType.Fly | MoveType.Walk,
                _ => default
            };

            foreach (var unit in sortedArray)
            {
                var isDead = unit.isDead;
                if (unit.TryGetComponent<ISummonbable>(out var summonbable))
                {
                    var isSummoned = summonbable.isSummoned;
                    if (!isSummoned) continue;
                }
                var oppoSide = unit.GetUnitSide(controller.ownerID);
                if (isDead || oppoSide == Side.PlayerSide) continue;
                if(unit is IMonster || unit is IPlayer)
                {
                    var oppoMoveType = unit.moveType;
                    if ((effectiveSide & oppoMoveType) == 0) continue;
                }
                var inRange = IsInRange(rangeAttackObj, rangeX, rangeZ, unit);
                if (!inRange) continue;
                filteredList.Add(unit);
            }
            return filteredList;
        };
        return getUnitsInWeponRange;
    }

    public static bool IsInRange(GameObject rangeAttackObj, float rangeX, float rangeZ, UnitBase other)
    {
        var vector = other.transform.position - rangeAttackObj.transform.position;

        var direction = vector.normalized;
        // Debug.Log(direction);
        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * rangeX, 2) + Mathf.Pow(direction.z * rangeZ, 2));

        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.rangeX, 2) + Mathf.Pow(direction.z * other.rangeZ, 2));

        float minDistance = effectiveRadius_me + effectiveRadius_other;
        var flatVector = new Vector3(vector.x, 0f, vector.z);
        var distance = flatVector.magnitude;

        if (distance <= minDistance) return true;
        else return false;
    }
}
