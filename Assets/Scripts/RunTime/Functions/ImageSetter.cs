using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class ImageSetter
{
    public static void SetCardImageFromData(this MonoBehaviour card,CardData cardData)
    {
        var iconImage = card.GetComponent<Image>();
        if (iconImage != null) iconImage.sprite = cardData.Icon;
        var energyImage = card.transform.GetChild(0).GetComponent<Image>();
        if (energyImage != null) energyImage.sprite = cardData.EnergyImage;
    }

    public static void SetSkillImageFromData(this MonoBehaviour skillIcon,SkillIconData skillIconData)
    {
        var iconImage = skillIcon.GetComponent<Image>();
        if (iconImage != null) iconImage.sprite = skillIconData.IconImage;
        var energyImage = skillIcon.transform.GetChild(0).GetComponent<Image>();
        if (energyImage != null) energyImage.sprite = skillIconData.EnergyImage;
    }
}
