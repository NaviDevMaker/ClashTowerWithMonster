
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
{
    public class CardImage
    {
        public Image iconImage;
        public Image energyImage;

        public Vector3 originalScale;
        public Color originalColor_icon;
        public Color originalColor_energy;

        public float scaleAmount = 1.3f;
        public float alphaAmount = 0.25f;

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

    private void  Awake()
    {
        SetCardImageFromData();
        var iconImage = GetComponent<Image>();
        var energyImage = transform.GetChild(0).GetComponent<Image>();
        cardImage = new CardImage { iconImage = iconImage, originalScale = transform.localScale, originalColor_icon = iconImage.color
            ,energyImage = energyImage,originalColor_energy = energyImage.color};
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
    public void OnPointerUp(PointerEventData eventData)
    {

    }
    private void OnDisable()
    {
        if(!CardSelected && !isSettedNextCard) cardImage.SetOriginal(this, isHandCard: !isSettedNextCard);
    }

    void SetCardImageFromData()
    {
        var iconImage = GetComponent<Image>();
        if (iconImage != null) iconImage.sprite = cardData.Icon;
        var energyImage = transform.GetChild(0).GetComponent<Image>();
        if (energyImage != null) energyImage.sprite = cardData.EnergyImage;
    }
}
