using System.Collections.Generic;
using UnityEngine;

public static class PoolObjectPreserver
{
    public static List<MeteoMover> meteoList = new List<MeteoMover>();

    public static MeteoMover MeteoGeter()
    {
        foreach (var meteo in meteoList)
        {
            if(!meteo.gameObject.activeInHierarchy && meteo.IsEndSpellProcess)
            {
                meteo.gameObject.SetActive(true);
                return meteo;
            }
        }

        return null;
    }
}
