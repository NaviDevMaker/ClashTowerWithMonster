using Game.Spells;
using System.Collections.Generic;
using UnityEngine;

public class SpellEffectHelper
{
    SpellBase spellBase;
    SpellType spellType;
    public SpellEffectHelper(SpellBase spellBase)
    {
        this.spellBase = spellBase;
        if (spellBase != null) spellType = spellBase._SpellStatus.SpellType;
    }
    void CompareEachUnit(UnitBase other)
    {
        if (!CompareUnitInRange(other)) return;
        EffectToEachUnit(other);
    }

    public bool CompareUnitInRange(UnitBase other)
    {
        var vector = other.transform.position - spellBase.transform.position;
        var direction = vector.normalized;
        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * spellBase.rangeX, 2) + Mathf.Pow(direction.z * spellBase.rangeZ, 2));
        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.rangeX, 2) + Mathf.Pow(direction.z * other.rangeZ, 2));

        //“G‚©‚ç‚Ì”¼Œa‚Æ©•ª‚Ì”¼Œa‚ğ‚Â‚È‚°‚½‚Æ‚«i‚¨Œİ‚¢‚ª”ÍˆÍŠO‚¬‚è‚¬‚èj‚Ì’·‚³
        //‚±‚êˆÈã”ÍˆÍ‚É“ü‚Á‚Ä‚¢‚½ê‡A”ÍˆÍ“à‚É‚Í‚¢‚Á‚Ä‚¢‚é‚Æ‚¢‚¤‚±‚Æ‚É‚È‚é
        float minDistance = effectiveRadius_me + effectiveRadius_other;

        var flatVector = new Vector3(vector.x, 0f, vector.z);
        var distance = flatVector.magnitude;
        if (distance <= minDistance) return true;
        else return false;
    }
    public void EffectToEachUnit(UnitBase other)
    {
       var effectAmount = spellBase._SpellStatus.EffectAmont;
        var canDamageToUnit = spellType.HasFlag(SpellType.Damage) || spellType.HasFlag(SpellType.DamageToEveryThing);
       if(canDamageToUnit)
       {
           if (other.TryGetComponent<IUnitDamagable>(out var damageable))
           {
               damageable.Damage(effectAmount);
           }
       }
       else if(spellBase._SpellStatus.SpellType == SpellType.Heal)
       {
           if(other.TryGetComponent<IUnitHealable>(out var healable))
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
    public List<UnitBase> GetUnitInRange()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(spellBase.gameObject, spellBase.prioritizedRange);
        if (sortedArray.Length == 0) return new List<UnitBase>();
        List<UnitBase> filteredList = new List<UnitBase>();
        foreach (var unit in sortedArray)
        {
            if (unit is ISummonbable summonbable && !summonbable.isSummoned) continue;
            var isDead = unit.isDead;
            var unitSide = unit.GetUnitSide(spellBase.ownerID);
            var effectSide = spellType switch
            {
                SpellType.Damage => Side.EnemySide,
                SpellType.Heal => Side.PlayerSide,
                SpellType.DamageToEveryThing or SpellType.OtherToEverything => Side.EnemySide | Side.PlayerSide,
                SpellType.OtherToPlayerside => Side.PlayerSide,
                SpellType.OtherToEnemyside => Side.EnemySide,
                _ => default
            };
            if (isDead || (effectSide & unitSide) == 0) continue;
            if (unit is IInvincible invincible && invincible.IsInvincible) continue;
            filteredList.Add(unit);
        }
        return filteredList;
    }
}
