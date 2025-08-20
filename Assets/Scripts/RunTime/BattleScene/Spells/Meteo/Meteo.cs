using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Spells.Meteo
{
    public class Meteo : SpellBase
    {
        int targetUnitCount = 3;
        List<MeteoMover> meteoList = new List<MeteoMover>();
        GameObject meteoPrefab;
        protected override async void Initialize()
        {
            _SpellStatus = await SetFieldFromAssets.SetField<SpellStatus>("Datas/Spells/Meteo");
            base.Initialize();
            pushEffectUnit = PushEffectUnit.OnlyEnemyUnit;
            meteoPrefab = await SetFieldFromAssets.SetField<GameObject>("Spells/Meteo");
        }

        protected override void SetDuration() => spellDuration = _SpellStatus.SpellDuration;
        protected override void SetRange()
        {
            scaleAmount = 1f;
            base.SetRange();
        }
        protected override async UniTaskVoid Spell()
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(spellDuration), cancellationToken: this.GetCancellationTokenOnDestroy());
                var targetUnits = spellEffectHelper.GetUnitInRange();
                var filterdList = targetUnits.Where(unit => spellEffectHelper.CompareUnitInRange(unit))
                .OrderByDescending(unit => unit.currentHP).ToList();

                if (filterdList.Count == 0) return;
                if (filterdList.Count > targetUnitCount)
                {
                    while (filterdList.Count != targetUnitCount)
                    {
                        var last = filterdList.Count - 1;
                        filterdList.RemoveAt(last);
                        await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
                    }
                }
                var count = filterdList.Count;
                meteoList = GetMeteoList(count);

                var tasks = new List<UniTask>();
                for (int i = 0; i < meteoList.Count; i++)
                {
                    var target = filterdList[i];
                    var meteo = meteoList[i];

                    var offsetY = Vector3.up * 10f;
                    var pos = target.transform.position + offsetY;
                    meteo.transform.position = pos;
                    meteo.attacker = this;
                    meteo.IsEndSpellProcess = false;
                    meteo.StartMove(target);
                    var task = UniTask.WaitUntil(() => meteo.IsEndSpellProcess, cancellationToken: this.GetCancellationTokenOnDestroy());
                    tasks.Add(task);
                }

                await UniTask.WhenAll(tasks);
            }
            catch (OperationCanceledException) { return; }
            DestroyAll();
        }

        List<MeteoMover> GetMeteoList(int count)
        {
            var list = new List<MeteoMover>();
            for (int i = 0; i < count; i++)
            {
                var meteo = PoolObjectPreserver.MeteoGeter();
                if (meteo == null)
                {
                    var newMeteo = Instantiate(meteoPrefab);
                    meteo = newMeteo.GetComponent<MeteoMover>();
                    list.Add(meteo);
                    PoolObjectPreserver.meteoList.Add(meteo);
                    continue;
                }
                list.Add(meteo);
            }
            return list;
        }

        protected override async void DestroyAll()
        {
            var tasks = new List<UniTask>();
            for (int i = 0; i < meteoList.Count; i++)
            {
                var meteo = meteoList[i];
                meteo.attacker = null;
                tasks.AddRange(meteo.PaticleDisapperTasksGeter());
            }
            await UniTask.WhenAll(tasks);
            if (this == null) return;
             Destroy(gameObject);
        }
    }

}

