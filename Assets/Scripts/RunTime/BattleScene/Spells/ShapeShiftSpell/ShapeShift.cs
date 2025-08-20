using Cysharp.Threading.Tasks;
using Game.Monsters;
using Game.Players;
using Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public class ShapeShift : SpellBase
{
    GameObject particleObj = null;
    Vector3 originalScale = Vector3.one;
    SelectableMonstersList selectableMonstersList = null;
    protected override async void Initialize()
    {
        _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/ShapeShiftSpell/ShapeShiftSpell");
        base.Initialize();
        particle = transform.GetChild(0).GetComponent<ParticleSystem>();
        particleObj = await SetFieldFromAssets.SetField<GameObject>("Effects/ShapeShiftSpellEffect");
        selectableMonstersList = await SetFieldFromAssets.SetField<SelectableMonstersList>("Datas/Spells/ShapeShiftSpell/Monsters.asset");
    }

    protected override void SetRange()
    {
        scaleAmount = 1.5f;
        base.SetRange();
    }
    protected override async UniTaskVoid Spell()
    {
        Debug.Log("•Ïg‚³‚¹‚Ü‚·");
        particle.Play();
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(spellDuration), cancellationToken: this.GetCancellationTokenOnDestroy());
            var units = spellEffectHelper.GetUnitInRange();
            var filteredList = units.Where(unit =>
            {
                var inRange = spellEffectHelper.CompareUnitInRange(unit);
                var isNotTower = !(unit is TowerControlller);
                var isNotPlayer = !(unit is IPlayer);
                return inRange && isNotTower && isNotPlayer;
            }).ToList();

            if (filteredList.Count == 0) return;
            var pList = new List<ParticleSystem>();
            for (int i = 0; i < filteredList.Count; i++)
            {
                var unit = filteredList[i];
                var pObj = PoolObjectPreserver.TransformerEffectGetter();
                if (pObj == null)
                {
                    pObj = Instantiate(particleObj);
                    PoolObjectPreserver.transformerEffectList.Add(pObj);
                }
                var pos = unit.gameObject.transform.position;
                pObj.transform.position = pos;

                var p = pObj.GetComponent<ParticleSystem>();
                pList.Add(p);
                SummonNewMonster(unit, pos, p);
                unit.gameObject.SetActive(false);
                unit.isDead = true;
            }

            var tasks = new List<UniTask>();
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            tasks.Add(task);
            pList.ForEach(p =>
            {
                var listTask = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
                tasks.Add(listTask);
            });
            await UniTask.WhenAll(tasks);
            pList.ForEach(p =>
            {
                p.transform.localScale = originalScale;
                p.gameObject.SetActive(false);
            });
        }
        catch (OperationCanceledException) { return; }
        DestroyAll();
    }
    protected override void SetDuration() => spellDuration = _SpellStatus.SpellDuration;

    protected override void DestroyAll()
    {
        if (this == null) return;
       Destroy(this.gameObject);
    }
    void SummonNewMonster(UnitBase transformedUnit,Vector3 pos,ParticleSystem particle)
    {
        var list = selectableMonstersList.SelectableMonsters;
        var r = UnityEngine.Random.Range(0, list.Count);
        var newMonster = list[r];
        var Id = transformedUnit.ownerID;
        var monster = Instantiate(newMonster,pos,Quaternion.identity);
        monster.ownerID = Id;
        var unitSize = monster.BodyMesh.bounds.size;
        var newScale = new Vector3(originalScale.x *unitSize.x,originalScale.y * unitSize.y,
            originalScale.z * unitSize.z);
        particle.transform.localScale = newScale;
        particle.Play();
        if (monster.TryGetComponent<ISummonbable>(out var summonbable))
        {
            summonbable.isSummoned = true;
        }
    }
}
