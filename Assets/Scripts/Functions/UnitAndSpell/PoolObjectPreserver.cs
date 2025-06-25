using System.Collections.Generic;
using UnityEngine;

public static class PoolObjectPreserver
{
    public static List<GameObject> meteoList = new List<GameObject>();

    public static GameObject MeteoGeter()
    {
        foreach (GameObject meteo in meteoList)
        {
            if(!meteo.activeInHierarchy)
            {
                meteo.gameObject.SetActive(true);
                return meteo;
            }
        }

        return null;
    }
}
