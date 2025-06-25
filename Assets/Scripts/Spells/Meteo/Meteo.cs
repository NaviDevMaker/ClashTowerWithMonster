using Cysharp.Threading.Tasks;
using Game.Spells;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : SpellBase
{
    int targetUnitCount = 3;
    GameObject meteoPrefab;
    protected override async void Initialize()
    {
        base.Initialize();
        meteoPrefab = await SetFieldFromAssets.SetField<GameObject>("Spells/Meteo");
    }

    protected override void SetDuration() => spellDuration = 0.3f;
    protected override async UniTaskVoid Spell()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(spellDuration));
        var meteoList = GetMeteoList();
        var targetUnits = spellEffectHelper.GetUnitInRange();
    }

    List<MeteoMover> GetMeteoList()
    {
        var list = new List<MeteoMover>();
        for (int i = 0; i < targetUnitCount; i++)
        {
            var meteo = PoolObjectPreserver.MeteoGeter();
            MeteoMover cmp = null;
            if (meteo == null)
            {
                var newMeteo = Instantiate(meteoPrefab);
                cmp = newMeteo.GetComponent<MeteoMover>();
                list.Add(cmp);
                PoolObjectPreserver.meteoList.Add(newMeteo);
            }
            cmp = meteo.GetComponent<MeteoMover>();
            list.Add(cmp);
        }

        return list;
    }


}
