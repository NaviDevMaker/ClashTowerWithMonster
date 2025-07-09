using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
public class NeedEnergyDisplayer : MonoBehaviour
{
    Text numeratorText;
    Text slashText;
    Text denominatorText;

    void Update()
    {
        RotateToCameraDirection();
    }

    void RotateToCameraDirection()
    {
        UIFuctions.LookToCamera(this.gameObject);
        var adjust = Quaternion.Euler(0f, 180f, 0f);
        gameObject.transform.rotation *= adjust;
    }
    public void SetText()
    {
        var texts = GetComponentsInChildren<Text>(includeInactive:true);
        var soretedTexts = texts.OrderBy(text =>
        {
           return text.gameObject.transform.GetSiblingIndex();
        }).ToList();

        for (int i = 0; i < soretedTexts.Count;i++)
        {
            switch (i)
            {
                case 0:
                numeratorText = soretedTexts[i]; break;
                case 1:
                slashText = soretedTexts[i]; break;
                case 2: denominatorText = soretedTexts[i]; break;
                default: break;
            }
        }
    }
    public void RenewEnergyText(int currentEnergy,int requiredEnergy)
    {
        var currentTextEnergy = int.Parse(numeratorText.text);
        denominatorText.text = requiredEnergy.ToString();
        if (currentTextEnergy == requiredEnergy)
        {
            FadeOutText();
            return;
        }

        Text[] texts = { numeratorText, slashText, denominatorText };
        texts.ToList().ForEach(text => SetOriginalColor(text));

        if (currentTextEnergy != currentEnergy)
        {
            numeratorText.text = currentEnergy.ToString();
            UIFuctions.ShakeUI(numeratorText);
        }
    }

    void FadeOutText()
    {
        Text[] texts = { numeratorText, slashText, denominatorText };
        var duration = 0.3f;
        texts.ToList().ForEach(text => FadeProcessHelper.FadeOutText(text,duration).Forget());
    }

    void SetOriginalColor(Text text)
    {
        if (text.color.a == 1.0f) return;
        Debug.Log("êFÇÇ‡Ç∆Ç…ñﬂÇµÇ‹ÇµÇΩ");
        var color = text.color;
        color.a = 1.0f;
        text.color = color;
    }
}
