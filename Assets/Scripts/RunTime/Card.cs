
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

public interface IIconImageInfo
{
    Image iconImage { get;}
    Image energyImage { get; }

    Color originalColor_icon { get; }
    Color originalColor_energy {  get; }
    float alphaAmount { get; }

    void SetNewAlpha();
    void SetShaderMaterialColor();
}

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler
{
    public class CardImage:IIconImageInfo
    {
        public Image iconImage { get; set; }
        public Image energyImage { get; set; }

        public Vector3 originalScale;
        public Color originalColor_icon { get; set; }
        public Color originalColor_energy { get; set; }

        public float scaleAmount = 1.3f;
        public float alphaAmount { get; set; } = 0.25f;

        bool isMeetedEnergy = false;
        public bool _isMeetedEnergy
        {
            get => isMeetedEnergy;
            set
            {
                if (value == isMeetedEnergy) return;
                isMeetedEnergy = value;
                SetShaderMaterialColor();
            }
        }
        public void SetNewAlpha()
        {
            Color color = originalColor_icon;
            Color color2 = originalColor_energy;
            color.a = alphaAmount;
            color2.a = alphaAmount;
            iconImage.color = color;
            energyImage.color = color2;
        }

        public void SetOriginal(Card card,bool isHandCard)
        {
            if(isHandCard)
            {
                card.gameObject.transform.localScale = originalScale;
                iconImage.color = originalColor_icon;
                energyImage.color = originalColor_energy;
            }
            else
            {
                iconImage.color = originalColor_icon;
                energyImage.color = originalColor_energy;
            }     
        }
        public void SetShaderMaterialColor()
        {
            var value = 0f;
            if (isMeetedEnergy) value = 1.0f;
            iconImage.material.SetFloat("_RevealAmount", value);
            energyImage.material.SetFloat("_RevealAmount", value);
        }
    }

    // [SerializeField] string monsterName;//test—p
    [SerializeField] CardData cardData;

    CardImage cardImage;
    public UnityAction<Card> OnSelectedCard;
    public static bool CardSelected { get; set; } = false;
    public bool isSettedNextCard = false;
    public CardName MonsterName { get => CardData.CardName;}
    public CardData CardData { get => cardData;}
    public CardImage _cardImage { get => cardImage;}
    public bool currentSelected { get; set; } = false;
    public Func<int> GetCurrentEnergy_Card;
    private void Update()
    {
        var requiredEnergy = cardData.Energy;
        if(GetCurrentEnergy_Card?.Invoke() >= requiredEnergy) cardImage._isMeetedEnergy = true;
        else cardImage._isMeetedEnergy = false;
    }
    public void Initialize()
    {
        this.SetCardImageFromData(cardData);
        var iconImage = GetComponent<Image>();
        var energyImage = transform.GetChild(0).GetComponent<Image>();
        cardImage = new CardImage
        {
            iconImage = iconImage,
            originalScale = transform.localScale,
            originalColor_icon = iconImage.color
            ,
            energyImage = energyImage,
            originalColor_energy = energyImage.color
        };

        iconImage.material = new Material(iconImage.material);
        energyImage.material = new Material(energyImage.material);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSettedNextCard) return;
        if(!CardSelected)
        {
            transform.localScale *= cardImage.scaleAmount;
        }
        cardImage.SetNewAlpha();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSettedNextCard) return;
        if(!currentSelected && !CardSelected) cardImage.SetOriginal(this,isHandCard:!isSettedNextCard);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isSettedNextCard) return;
        Debug.Log("‰Ÿ‚³‚ê‚Ü‚µ‚½");
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log(MonsterName.ToString());
            OnSelectedCard?.Invoke(this);

            currentSelected = true;
            transform.localScale = cardImage.originalScale * cardImage.scaleAmount;
            cardImage.SetNewAlpha();
        }
    }
    
    private void OnDisable()
    {
        if(!CardSelected && !isSettedNextCard) cardImage.SetOriginal(this, isHandCard: !isSettedNextCard);
    }

    //void SetCardImageFromData()
    //{
    //    var iconImage = GetComponent<Image>();
    //    if (iconImage != null) iconImage.sprite = cardData.Icon;
    //    var energyImage = transform.GetChild(0).GetComponent<Image>();
    //    if (energyImage != null) energyImage.sprite = cardData.EnergyImage;
    //}


}
