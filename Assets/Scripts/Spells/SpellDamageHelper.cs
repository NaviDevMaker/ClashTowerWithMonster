using Game.Spells;
using System.Collections.Generic;
using UnityEngine;

public class SpellDamageHelper
{
    SpellBase spellBase;
    public SpellDamageHelper(SpellBase spellBase)
    {
        this.spellBase = spellBase;
    }
    void CompareEachUnit(UnitBase other)
    {
        var vector = other.transform.position - spellBase.transform.position;

        var direction = vector.normalized;

        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * spellBase.rangeX, 2) + Mathf.Pow(direction.z * spellBase.rangeZ, 2));

        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.rangeX, 2) + Mathf.Pow(direction.z * other.rangeZ, 2));

        //“G‚©‚ç‚Ì”¼Œa‚Æ©•ª‚Ì”¼Œa‚ğ‚Â‚È‚°‚½‚Æ‚«i‚¨Œİ‚¢‚ª”ÍˆÍŠO‚¬‚è‚¬‚èj‚Ì’·‚³
        //‚±‚êˆÈã”ÍˆÍ‚É“ü‚Á‚Ä‚¢‚½ê‡A”ÍˆÍ“à‚É‚Í‚¢‚Á‚Ä‚¢‚é‚Æ‚¢‚¤‚±‚Æ‚É‚È‚é
        float minDistance = effectiveRadius_me + effectiveRadius_other;
        var distance = vector.magnitude;

        DamageEachUnit(other, distance, minDistance, direction);
    }

    void DamageEachUnit(UnitBase other, float distance, float minDistance, Vector3 direction)
    {
        if (distance < minDistance)
        {
           if(other.TryGetComponent<IUnitDamagable>(out var damageable))
           {
                damageable.Damage(spellBase.SpellStatus.Damage);
           }
        }
    }
    public void DamageToUnit()
    {
        var filteredList = GetUnitInRange();
      
        if (filteredList.Count == 0) return;
        filteredList.ForEach(cmp => CompareEachUnit(cmp));
    }
    List<UnitBase> GetUnitInRange()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(spellBase.gameObject, spellBase.prioritizedRange);
        if (sortedArray.Length == 0) return new List<UnitBase>();
        List<UnitBase> filteredList = new List<UnitBase>();
        foreach (var unit in sortedArray)
        {
            var isDead = unit.isDead;
            var playerSide = unit.Side == Side.PlayerSide;
            if (isDead || playerSide) continue;
            filteredList.Add(unit);
        }

        return filteredList;
    }
}
