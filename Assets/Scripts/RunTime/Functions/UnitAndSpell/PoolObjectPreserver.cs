using System.Collections.Generic;
using UnityEngine;
using Game.Spells.Meteo;
public static class PoolObjectPreserver
{
    public static List<MeteoMover> meteoList = new List<MeteoMover>();
    public static List<LineRenderer> lineRenderers = new List<LineRenderer>();
    public static List<GameObject> transformerEffectList = new List<GameObject>();
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
    public static GameObject TransformerEffectGetter()
    {
        foreach (var effect in transformerEffectList)
        {
            if (!effect.activeSelf)
            {
                effect.gameObject.SetActive(true);
                return effect;
            }
        }
            return null;
    }
}
