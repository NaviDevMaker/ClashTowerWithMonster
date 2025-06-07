using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public  class AddForceToUnit<T> where T : MonoBehaviour, IPushable
{
    T me;
    float pushAmount = 10f;
    public AddForceToUnit(T me)
    {
        this.me = me;
    }

    void PushEachUnit(UnitBase other)
    {
        var vector = other.pushbleTransform.position - me.pushbleTransform.position;
     
        var direction = vector.normalized;

        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * me.radiusX, 2) + Mathf.Pow(direction.z * me.radiusZ, 2));

        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.radiusX, 2) + Mathf.Pow(direction.z * other.radiusZ, 2));

        //“G‚©‚ç‚Ì”¼Œa‚Æ©•ª‚Ì”¼Œa‚ğ‚Â‚È‚°‚½‚Æ‚«i‚¨Œİ‚¢‚ª”ÍˆÍŠO‚¬‚è‚¬‚èj‚Ì’·‚³
        //‚±‚êˆÈã”ÍˆÍ‚É“ü‚Á‚Ä‚¢‚½ê‡A”ÍˆÍ“à‚É‚Í‚¢‚Á‚Ä‚¢‚é‚Æ‚¢‚¤‚±‚Æ‚É‚È‚é
        float minDistance = effectiveRadius_me + effectiveRadius_other;
        var distance = vector.magnitude;
        if(distance < minDistance)
        {
            var extraDistance = minDistance - distance;
            var push = direction * extraDistance;
            if (other.GetType() == typeof(TowerControlller))
            {
                var targetPos = me.pushbleTransform.position - push;
                me.pushbleTransform.position = Vector3.MoveTowards(me.pushbleTransform.position, targetPos, pushAmount * Time.deltaTime);
            }
            else
            {              
                var targetPos_me = me.pushbleTransform.position - push / 2;
                var targetPos_other = other.pushbleTransform.position + push / 2;
                me.pushbleTransform.position = Vector3.MoveTowards(me.pushbleTransform.position, targetPos_me, pushAmount * Time.deltaTime);             
                other.transform.position = Vector3.MoveTowards(other.pushbleTransform.position, targetPos_other, pushAmount * Time.deltaTime);//targetPos;//Vector3.MoveTowards(me.transform.position, targetPos,pushAmount *Time.deltaTime);
            }          
        }           
    }

    public void KeepDistance()
    {
        var filteredList = GetUnitInRange();
        if (filteredList.Count == 0) return;
        filteredList.ForEach(cmp => PushEachUnit(cmp));
    }

    List<UnitBase> GetUnitInRange()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(me.gameObject, me.prioritizedRadius);
        if (sortedArray.Length == 0) return default;
        List<UnitBase> filteredList = new List<UnitBase>();
        var myType = me.moveType;
        foreach (var unit in sortedArray)
        {
            var isDead = unit.isDead;
            var fly = unit.moveType == MoveType.Fly;
            if (isDead) continue;
            if (myType == MoveType.Fly && !fly) continue;
            filteredList.Add(unit);
        }

        return filteredList;
    }
}
