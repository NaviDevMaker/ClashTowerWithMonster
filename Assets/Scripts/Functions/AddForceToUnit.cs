using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;


public  class AddForceToUnit<T> where T : MonoBehaviour, IPushable
{
    T me;
    float pushAmount;
    public AddForceToUnit(T me,float pushAmount)
    {
        this.me = me;
        this.pushAmount = pushAmount;
    }

    void CompareEachUnit(UnitBase other)
    {
        var vector = other.transform.position - me.transform.position;
     
        var direction = vector.normalized;

        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * me.rangeX, 2) + Mathf.Pow(direction.z * me.rangeZ, 2));

        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.rangeX, 2) + Mathf.Pow(direction.z * other.rangeZ, 2));

        //敵からの半径と自分の半径をつなげたとき（お互いが範囲外ぎりぎり）の長さ
        //これ以上範囲に入っていた場合、範囲内にはいっているということになる
        float minDistance = effectiveRadius_me + effectiveRadius_other;
        var distance = vector.magnitude;

        PushEachUnit(other,distance,minDistance,direction);      
    }

    async void PushEachUnit(UnitBase other, float distance,float minDistance,Vector3 direction)
    {
        if (distance < minDistance)
        {
            var extraDistance = minDistance - distance;
            var push = direction * extraDistance;
            var otherType = other.GetType();

            Vector3 targetPos_me = Vector3.zero;
            Vector3 targetPos_other = Vector3.zero;
            if (otherType == typeof(TowerControlller))
            {
                targetPos_me = me.transform.position - push;
                me.transform.position = Vector3.MoveTowards(me.transform.position, targetPos_me, pushAmount * Time.deltaTime);
            }
            else if ((other is IPlayer || other is IMonster) && me is ISpells)
            {
                Debug.Log("呪文発動");
                targetPos_other = other.transform.position - push;
                other.transform.position = Vector3.MoveTowards(other.transform.position, targetPos_other, pushAmount * Time.deltaTime);
            }
            else if(other is IPlayer || other is IMonster)
            {
                Debug.Log("押された！！！！！");
                me.isKnockBacked = true;
                targetPos_me = me.transform.position - push / 2;
                targetPos_other = other.transform.position + push / 2;
                me.transform.position = Vector3.MoveTowards(me.transform.position, targetPos_me,pushAmount * Time.deltaTime);
                other.transform.position = Vector3.MoveTowards(other.transform.position, targetPos_other,pushAmount * Time.deltaTime);
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f)); // 例: 0.2秒間ノックバック中
                me.isKnockBacked = false;
            }
        }
    }
    public void KeepDistance(MoveType moveType)
    {
        Debug.Log(moveType);
        List<UnitBase> filteredList = new List<UnitBase>();
        filteredList = moveType switch
        {
            MoveType.Walk or MoveType.Fly => GetUnitInRange_Monster(),
            MoveType.Spell => GetUnitInRange_Spell(),
            _ => filteredList

        };
        if (filteredList.Count == 0) return;
        filteredList.ForEach(cmp => CompareEachUnit(cmp));
    }

    List<UnitBase> GetUnitInRange_Monster()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(me.gameObject, me.prioritizedRange);
        if (sortedArray.Length == 0) return new List<UnitBase>();
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

    List<UnitBase> GetUnitInRange_Spell()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(me.gameObject, me.prioritizedRange);
        if (sortedArray.Length == 0) return new List<UnitBase>();
        List<UnitBase> filteredList = new List<UnitBase>();
        foreach (var unit in sortedArray)
        {
            var isDead = unit.isDead;
            var playerSide = unit.Side == Side.PlayerSide;
            var isTower = unit.UnitType == UnitType.tower;
            if (isDead || playerSide || isTower) continue;
            filteredList.Add(unit);
        }

        return filteredList;
    }
}
