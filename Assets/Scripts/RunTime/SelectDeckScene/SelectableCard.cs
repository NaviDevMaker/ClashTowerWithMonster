using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableCard : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    public class SelectableCardImage
    {
        public Image iconImage;

        public Vector3 originalScale;
    
        public float scaleAmount = 1.3f;
        public void SetOriginalScale() => iconImage.rectTransform.localScale = originalScale;
        public void SetScale() => iconImage.rectTransform.localScale = originalScale * scaleAmount;
    }

    [SerializeField] CardData cardData;
    SelectableCardImage selectableCardImage;

    private void Start()
    {
        Initialize();
    }
    void Initialize()
    {
        this.SetCardImageFromData(cardData);
        var iconImage = GetComponent<Image>();
        var originalScale = iconImage.rectTransform.localScale;
        selectableCardImage = new SelectableCardImage { iconImage = iconImage, originalScale = originalScale };
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        selectableCardImage?.SetScale();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        selectableCardImage?.SetOriginalScale();
    }
}
