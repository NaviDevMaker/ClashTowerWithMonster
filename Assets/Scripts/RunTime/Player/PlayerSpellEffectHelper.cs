using Game.Players;
using Game.Spells;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSpellEffectHelper
{
    int effectAmount = 0;
    GameObject origin;
    SpellType spellType;
    float rangeX = 0f;
    float rangeZ = 0f;
    float prioritizedRange = 0f;
    float timerOffsetY = 0f;
   public PlayerSpellEffectHelper(int effectAmount,SpellType spellType,GameObject colliderObj)
   {
        this.effectAmount = effectAmount;
        this.spellType = spellType;
        origin = colliderObj;
        SetRange(colliderObj);
   }


    void SetRange(GameObject colliderObj)
    {
        IColliderRangeProvider colliderRangeProvider = null;

        if (colliderObj.TryGetComponent<BoxCollider>(out var boxCollider))
        {
            colliderRangeProvider = new BoxColliderrangeProvider { boxCollider = boxCollider };
            rangeX = colliderRangeProvider.GetRangeX();
            rangeZ = colliderRangeProvider.GetRangeZ();
            prioritizedRange = colliderRangeProvider.GetPriorizedRange();
            timerOffsetY = colliderRangeProvider.GetTimerOffsetY();
        }
        else if (colliderObj.TryGetComponent<SphereCollider>(out var sphereCollider))
        {
            colliderRangeProvider = new SphereColliderRangeProvider { sphereCollider = sphereCollider };
            var scaleAmount = sphereCollider.gameObject.transform.localScale.magnitude;
            rangeX = colliderRangeProvider.GetRangeX() * scaleAmount;// 
            rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount;//
            prioritizedRange = colliderRangeProvider.GetPriorizedRange() * scaleAmount;// 
            Debug.Log($"{scaleAmount},{rangeX},{rangeZ},{prioritizedRange}");
            timerOffsetY = colliderRangeProvider.GetTimerOffsetY() * scaleAmount;
        }
        else return;
    }

    List<UnitBase> GetUnitInRange()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(origin,prioritizedRange);
        if (sortedArray.Length == 0) return new List<UnitBase>();
        var filteredList = new List<UnitBase>();
        foreach (var unit in sortedArray)
        {
            var isDead = unit.isDead;
            var unitSide = unit.Side;
            var effectSide = spellType switch
            {
                SpellType.Damage => Side.EnemySide,
                SpellType.Heal => Side.PlayerSide,
                SpellType.DamageToEveryThing => Side.EnemySide | Side.PlayerSide,
                _ => default
            };


            if (isDead || (effectSide & unitSide) == 0) continue;
            filteredList.Add(unit);
        }

        return filteredList;
    }

    void CompareEachUnit(UnitBase other)
    {
        if (!CompareUnitInRange(other)) return;
        EffectToEachUnit(other);
    }

    bool CompareUnitInRange(UnitBase other)
    {
        var vector = other.transform.position - origin.transform.position;
        var direction = vector.normalized;
        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * rangeX, 2) + Mathf.Pow(direction.z * rangeZ, 2));
        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.rangeX, 2) + Mathf.Pow(direction.z * other.rangeZ, 2));

        //ìGÇ©ÇÁÇÃîºåaÇ∆é©ï™ÇÃîºåaÇÇ¬Ç»Ç∞ÇΩÇ∆Ç´ÅiÇ®å›Ç¢Ç™îÕàÕäOÇ¨ÇËÇ¨ÇËÅjÇÃí∑Ç≥
        //Ç±ÇÍà»è„îÕàÕÇ…ì¸Ç¡ÇƒÇ¢ÇΩèÍçáÅAîÕàÕì‡Ç…ÇÕÇ¢Ç¡ÇƒÇ¢ÇÈÇ∆Ç¢Ç§Ç±Ç∆Ç…Ç»ÇÈ
        float minDistance = effectiveRadius_me + effectiveRadius_other;

        var flatVector = new Vector3(vector.x, 0f, vector.z);
        var distance = flatVector.magnitude;
        if (distance <= minDistance) return true;
        else return false;
    }
    void EffectToEachUnit(UnitBase other)
    {
        var canDamageToUnit = spellType.HasFlag(SpellType.Damage) || spellType.HasFlag(SpellType.DamageToEveryThing);
        if (canDamageToUnit)
        {
            if (other.TryGetComponent<IUnitDamagable>(out var damageable))
            {
                damageable.Damage(effectAmount);
            }
        }
        else if (spellType == SpellType.Heal)
        {
            if (other.TryGetComponent<IUnitHealable>(out var healable))
            {
                healable.Heal(effectAmount);
            }
        }
    }
    public void EffectToUnit()
    {
        var filteredList = GetUnitInRange();

        Debug.Log(filteredList.Count);
        if (filteredList.Count == 0) return;
        filteredList.ForEach(cmp => CompareEachUnit(cmp));
    }

}
