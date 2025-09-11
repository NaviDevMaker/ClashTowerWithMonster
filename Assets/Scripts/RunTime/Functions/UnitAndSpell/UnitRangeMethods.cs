using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Game.Monsters;
using Unity.VisualScripting;

public static class UnitRangeMethods
{
    public static  Func<List<UnitBase>> GetUnitInWeponRange<T>(this T controller,MoveType moveType = default) where T : UnitBase
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
                var scale = rangeAttackObj.transform.lossyScale;
                colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
                rangeX = colliderRangeProvider.GetRangeX() * scale.x;
                rangeZ = colliderRangeProvider.GetRangeZ() * scale.z;
                prioritizedRange = colliderRangeProvider.GetPriorizedRange();
                prioritizedRange = rangeX == prioritizedRange? prioritizedRange * scale.x : prioritizedRange * scale.z;
            }
        }

        Func<List<UnitBase>> getUnitsInWeponRange = () =>
        {
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(rangeAttackObj, prioritizedRange);
            if (sortedArray.Length == 0) return new List<UnitBase>();
            List<UnitBase> filteredList = new List<UnitBase>();
            var myType = moveType == default ? controller.moveType : moveType;
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

    public static Func<List<UnitBase>> GetUnitInSpecificRangeItem<T>(this T rangeAttackObj,UnitBase attacker = null
        ,MoveType moveType = default,int ownerID = default) where T : MonoBehaviour, IRangeAttack
    {
        if (!(rangeAttackObj is IRangeAttack rangeAttack))
        {
            Debug.LogError("インターフェイス継承してないよ俺、しっかりしてくれ...");
            return null;
        }
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
                var scale = rangeAttackObj.transform.lossyScale;
                colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
                rangeX = colliderRangeProvider.GetRangeX() * scale.x;
                rangeZ = colliderRangeProvider.GetRangeZ() * scale.z;
                prioritizedRange = colliderRangeProvider.GetPriorizedRange();
                prioritizedRange = rangeX == prioritizedRange ? prioritizedRange * scale.x : prioritizedRange * scale.z;
            }
        }

        Func<List<UnitBase>> getUnitsInWeponRange = () =>
        {
            var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(rangeAttackObj.gameObject, prioritizedRange);
            if (sortedArray.Length == 0) return new List<UnitBase>();
            List<UnitBase> filteredList = new List<UnitBase>();
            var myType = moveType == default ? attacker.moveType : moveType;
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
                var ID = attacker != null ? attacker.ownerID : ownerID;
                var oppoSide = unit.GetUnitSide(ID);
                if (isDead || oppoSide == Side.PlayerSide) continue;
                if (unit is IMonster || unit is IPlayer)
                {
                    var oppoMoveType = unit.moveType;
                    if ((effectiveSide & oppoMoveType) == 0) continue;
                }
                var inRange = IsInRange(rangeAttackObj.gameObject, rangeX, rangeZ, unit);
                if (!inRange) continue;
                filteredList.Add(unit);
            }
            return filteredList;
        };
        return getUnitsInWeponRange;
    }

    public static TowerController GetTargetTower(this GameObject controller,int ownerID)
    {
        Debug.Log("ターゲットのタワーを取得します");
        TowerController[] targetTowers = GameObject.FindObjectsByType<TowerController>(sortMode: FindObjectsSortMode.None);

        List<TowerController> toList = new List<TowerController>(targetTowers);
        toList = toList
            .Where(tower =>
            {
                var isDead = tower.isDead;
                var side = tower.GetUnitSide(ownerID);
                return !isDead && side == Side.EnemySide;
            })
            .OrderBy(tower => Vector3.Distance(controller.transform.position, tower.transform.position)).ToList();
        if (toList.Count > 0) return toList[0];
        else return null;
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
