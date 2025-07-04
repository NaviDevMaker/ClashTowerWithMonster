using UnityEngine;
using UnityEngine.UI;

public static class CardImageSetter
{
    public static void SetCardImageFromData(this MonoBehaviour card,CardData cardData)
    {
        var iconImage = card.GetComponent<Image>();
        if (iconImage != null) iconImage.sprite = cardData.Icon;
        var energyImage = card.transform.GetChild(0).GetComponent<Image>();
        if (energyImage != null) energyImage.sprite = cardData.EnergyImage;
    }
}
