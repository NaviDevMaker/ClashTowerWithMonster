using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableCard : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler,
    IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public class SelectableCardImage
    {
        public Image iconImage;
        public Button useButton;
        public Button statusButton;
        public Vector3 originalScale;

        float scaleUpAmount = 1.05f;
        float scaleDownAmount = 0.95f;

        public CancellationTokenSource cls = new CancellationTokenSource();
        public CancellationTokenSource buttonCls = new CancellationTokenSource();
        public async void OnPointerUp()
        {
            var duration = 0.075f;
            var endValue = originalScale * scaleUpAmount;
            var scaleUpSet = new Vector3TweenSetup(endValue, duration);
            var scaleDownSet = new Vector3TweenSetup(originalScale, duration);
            try
            {
                await iconImage.gameObject.Scaler(scaleUpSet).ToUniTask(cancellationToken:cls.Token);
                await iconImage.gameObject.Scaler(scaleDownSet).ToUniTask(cancellationToken: cls.Token);
            }
            catch (OperationCanceledException) { }
            finally { iconImage.rectTransform.localScale = originalScale; }
        }
            
        public void OnPointerDown() => iconImage.rectTransform.localScale = originalScale * scaleDownAmount;
        public async void OpenButtonUI()
        {
            var duration = 0.1f;
            var buttonScaleUp = new Vector3TweenSetup(Vector3.one, duration, Ease.Linear);
            var task = useButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            var task2 = statusButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            await UniTask.WhenAll(task, task2);
        }

        public async void CloseButtonUI()
        {
            var duration = 0.1f;
            var buttonScaleUp = new Vector3TweenSetup(Vector3.zero, duration, Ease.Linear);
            var task = useButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            var task2 = statusButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            await UniTask.WhenAll(task, task2);
        }
    }

    public CardData cardData { get; set; }//Ç±Ç±Ç…SelectCardManagerÇÃCardDataÇì¸ÇÍÇÈ
    ScrollRect scrollRect;
    SelectableCardImage selectableCardImage;
  
    bool isSelected = false;
    public bool _isSelected 
    { 
        get => isSelected; 
        set
        {
            if (value == isSelected) return;
            isSelected = value;
            if(!isSelected) selectableCardImage?.CloseButtonUI();
        }
    } 

    public UnityAction<bool> OnPointerDownStatus;
    public UnityAction<SelectableCard> OnSelectedCard;
    private void Start()
    {
        //Initialize();
    }
    public void Initialize(ScrollRect scrollRect,UnityAction<bool> stopScrollAction,UnityAction<SelectableCard> selectedCardChanged)
    {
        this.scrollRect = scrollRect;
        this.SetCardImageFromData(cardData);
        var iconImage = GetComponent<Image>();
        var useButton = iconImage.transform.GetChild(1).GetComponent<Button>(); 
        var statusButton = iconImage.transform.GetChild(2).GetComponent<Button>();
        var originalScale = iconImage.rectTransform.localScale;
        selectableCardImage = new SelectableCardImage {
            iconImage = iconImage,
            useButton = useButton,
            statusButton = statusButton,
            originalScale = originalScale
        };
        OnPointerDownStatus = stopScrollAction;
        OnSelectedCard = selectedCardChanged;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("DragèIóπ");
        scrollRect.OnEndDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("DragíÜ");
        scrollRect.OnDrag(eventData);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("DragäJén");
        scrollRect.OnBeginDrag(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerDownStatus?.Invoke(false);
        selectableCardImage.cls?.Cancel();
        selectableCardImage.cls?.Dispose();
        selectableCardImage.cls = new CancellationTokenSource();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownStatus?.Invoke(true);
        selectableCardImage?.OnPointerDown();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = true;
        selectableCardImage?.OnPointerUp();
        selectableCardImage?.OpenButtonUI();
        OnSelectedCard?.Invoke(this);
    }
}
