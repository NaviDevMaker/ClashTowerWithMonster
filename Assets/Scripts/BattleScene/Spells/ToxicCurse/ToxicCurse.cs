using Cysharp.Threading.Tasks;
using Game.Monsters.SpellDemon;
using Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToxicCurse : SpellBase
{
    List<ParticleSystem> particles = new List<ParticleSystem>();
    List<UnitBase> enteredUnit = new List<UnitBase>();
    GameObject demon;

    protected override async void Initialize()
    {
        demon = await SetFieldFromAssets.SetField<GameObject>("Monsters/SpellDemon");
        _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/ToxicCurse");
        base.Initialize();
        particle = GetComponent<ParticleSystem>();
        particles = GetComponentsInChildren<ParticleSystem>().ToList();
    }
    protected override void SetDuration() => spellDuration = 8f;
    protected override async UniTaskVoid Spell()
    {
        Debug.Log(spellDuration);
        if (particle != null) particle.Play();
        var time = 0f;
        var reminningTime = 0f;
        while(time < spellDuration)
        {
            Debug.Log("ああああああ");
            time += Time.deltaTime;
            reminningTime = Mathf.Max(0,spellDuration - time);
            var nowInRangeUnit = new List<UnitBase>();
            var list = spellEffectHelper.GetUnitInRange();
            var removedTowerList = list.Where(t =>
            {
                var tower = t is TowerControlller;
                if (tower) return false;
                return true;
            });
            foreach (var unit in removedTowerList)
            {
                if(CompareUnitInRange(unit, reminningTime)) nowInRangeUnit.Add(unit);
            }

            CheckOutOfRangeUnit(nowInRangeUnit);
            await UniTask.Yield();
        }
        DestroyAll();
    }

    
    protected override async void DestroyAll()
    {
        var tasks = new List<UniTask>();
        particles.ForEach(p =>
        {
           var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
           tasks.Add(task);
        });
        await UniTask.WhenAll(tasks);
        Destroy(gameObject);
    }
    protected override void SetRange()
    {
        scaleAmount = 3;
        base.SetRange();
    }
  

    bool CompareUnitInRange(UnitBase other,float reminningTime)
    {
        var vector = other.transform.position - transform.position;

        var direction = vector.normalized;

        float effectiveRadius_me = Mathf.Sqrt(Mathf.Pow(direction.x * rangeX, 2) + Mathf.Pow(direction.z * rangeZ, 2));

        float effectiveRadius_other = Mathf.Sqrt(Mathf.Pow(direction.x * other.rangeX, 2) + Mathf.Pow(direction.z * other.rangeZ, 2));

        //敵からの半径と自分の半径をつなげたとき（お互いが範囲外ぎりぎり）の長さ
        //これ以上範囲に入っていた場合、範囲内にはいっているということになる
        float minDistance = effectiveRadius_me + effectiveRadius_other;
        var distance = vector.magnitude;

        if (distance >= minDistance) return false;

        //if (other.statusCondition.DemonCurse.isActive)
        //{
        //    if (!enteredUnit.Contains(other)) enteredUnit.Add(other);
        //    return true;
        //}
        DemonProcess(other, distance, minDistance, direction,reminningTime);
        return true;
    }
    void DemonProcess(UnitBase other, float distance, float minDistance, Vector3 direction,float reminningTime)
    {
        if (other == null || other.isDead) return;
        if(other.TryGetComponent<ISummonbable>(out var summonbable))
        {
            var isSummoned = summonbable.isSummoned;
            if (!isSummoned) return;
        }

        if (other.statusCondition.DemonCurse.isEffectedCount != 0)
         {
            other.statusCondition.DemonCurse.isActive = true;
            return;
         }
         var body = 0;
         var bounds = other.MySkinnedMeshes[body].bounds;
         var center = bounds.center;
         var y = bounds.size.y;
         var pos = center;
         
         if(other is IPlayer)
         {
            var offsetY = 0.5f;
            y += offsetY;
         }

         pos.y = y;
         var rot = other.transform.rotation;
         var demonObj = Instantiate(demon, pos, rot);
        // var rawScale = demonObj.transform.lossyScale;
        //var otherScale = other.transform.lossyScale;
        //var correctScale = new Vector3(
        //   rawScale.x / otherScale.x,
        //   rawScale.y / otherScale.y,
        //   rawScale.z / otherScale.z
        //   );
        //Debug.Log($"{rawScale},{otherScale},{correctScale}");
        demonObj.transform.SetParent(other.transform);
        

        //demonObj.transform.localScale = correctScale;
        var controller = demonObj.GetComponent<SpellDemonController>();
        if (controller != null)
        {
            Debug.Log("デーモン召喚！！");
            enteredUnit.Add(other);
            controller.targetUnit = other;
            controller.duration = reminningTime;
            controller.EndSetProcess = true;
            other.statusCondition.DemonCurse.isActive = true;
            other.statusCondition.DemonCurse.isEffectedCount++;
        }
    }
    void CheckOutOfRangeUnit(List<UnitBase> nowInRangeUnit)
    {
        foreach (var unit in enteredUnit)
        {
            if(!nowInRangeUnit.Contains(unit)) unit.statusCondition.DemonCurse.isActive = false;
        }
    }
}
