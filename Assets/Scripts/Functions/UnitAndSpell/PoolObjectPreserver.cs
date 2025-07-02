using System.Collections.Generic;
using UnityEngine;

public static class PoolObjectPreserver
{
    public static List<MeteoMover> meteoList = new List<MeteoMover>();
    public static List<LineRenderer> lineRenderers = new List<LineRenderer>();
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

    public static LineRenderer LineRendererGetter()
    {
        foreach (var lineRenderer in lineRenderers)
        {
            if (!lineRenderer.gameObject.activeSelf)
            {
                //lineRenderer.gameObject.SetActive(false);
                return lineRenderer;
            }
        }

        return null;
    }
}
