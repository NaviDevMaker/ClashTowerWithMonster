using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SkillIcon : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    public class SkillImages : IIconImageInfo
    {
        public Image iconImage { get; set; }
        public Image energyImage { get; set; }

        public Color originalColor_icon { get; set; }

        public Color originalColor_energy { get; set; }

        public float alphaAmount { get; set; } = 0.25f;

        bool isMeetedEnergy { get; set; }

        public bool _isMeetedEnergy
        { 
            get => isMeetedEnergy;
            set
            {
                if (isMeetedEnergy == value) return;
                else
                {
                    isMeetedEnergy = value;
                    SetShaderMaterialColor();
                }
            }
        }
        public void SetNewAlpha()
        {
            var color1 = originalColor_icon;
            var color2 = originalColor_energy;
            color1.a = alphaAmount;
            color2.a = alphaAmount;
            iconImage.color = color1;
            energyImage.color = color2;
        }
        public void SetOriginal()
        {
            iconImage.color = originalColor_icon;
            energyImage.color = originalColor_energy;
        }
        public void SetShaderMaterialColor()
        {
            var value = isMeetedEnergy == true ? 1.0f : 0f;
            if (value == 1.0f) { var tween = UIFuctions.ScaleUI(iconImage); }
            iconImage.material.SetFloat("_RevealAmount", value);
            energyImage.material.SetFloat("_RevealAmount", value);
        }
    }

    [SerializeField] SkillIconData skillIconData;//将来的におそらくここにプレイヤーが選んだやつを入れるから
    [SerializeField] EnergyGageController energyGageController;

    SkillImages skillImages;
    private void Start()
    {
        energyGageController.ShakeUIAction = ShakeIconImage;
        this.SetSkillImageFromData(skillIconData);
        var iconImage = GetComponent<Image>();
        var energyImage = transform.GetChild(0).GetComponent<Image>();
        skillImages = new SkillImages
        {
            iconImage = iconImage,
            energyImage = energyImage,
            originalColor_icon = iconImage.color,
            originalColor_energy = energyImage.color,
        };

        iconImage.material = new Material(iconImage.material);
        energyImage.material = new Material(energyImage.material);
    }

    private void Update()
    {
        skillImages._isMeetedEnergy = energyGageController.currentEnergy >= skillIconData.Energy;
    }

    void ShakeIconImage() => UIFuctions.ShakeUI(skillImages.iconImage);
    public void OnPointerEnter(PointerEventData eventData) => skillImages.SetNewAlpha();
    public void OnPointerExit(PointerEventData eventData) => skillImages.SetOriginal();
}
