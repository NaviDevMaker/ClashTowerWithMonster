using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
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
        public class TweenFields
        {
            readonly float scaleUpAmount = 1.05f;

            public readonly float simpleScaleDuration = 0.075f;
            public readonly float buttonScaleDuration = 0.1f;
            public readonly float moveDuration = 1f;
            readonly Vector2 moveTarget = new Vector2(275f,-120f);
            public Vector3 endValue { get; private set; }
            public Vector3TweenSetup simpleScaleUpSet { get; private set; }
            public Vector3TweenSetup simpleScaleDownSet { get; private set; }
            public Vector3TweenSetup buttonScaleUpSet {  get; private set; }
            public Vector3TweenSetup buttonScaleDownSet { get; private set; }
            public Vector3TweenSetup scaleUpWhenMoveSet { get; private set; }
            public Vector2TweenSetup moveSet {  get; private set; }

            public FadeSet fadeOutSet { get; private set; }
            public FadeSet fadeInSet { get; private set; }
            public TweenFields(Vector3 originalScale)
            {
                endValue = originalScale * scaleUpAmount;
                simpleScaleUpSet = new Vector3TweenSetup(endValue, simpleScaleDuration);
                simpleScaleDownSet = new Vector3TweenSetup(originalScale, simpleScaleDuration);
                buttonScaleUpSet = new Vector3TweenSetup(Vector3.one, buttonScaleDuration,Ease.Linear);
                buttonScaleDownSet = new Vector3TweenSetup(Vector3.zero, buttonScaleDuration,Ease.Linear);
                scaleUpWhenMoveSet = new Vector3TweenSetup(Vector3.one, moveDuration);
                moveSet = new Vector2TweenSetup(moveTarget,moveDuration);
                fadeOutSet = new FadeSet(0f,moveDuration);
                fadeInSet = new FadeSet(1f,moveDuration);
            }
        }

        Image iconImage;
        Button useButton;
        Button statusButton;
        Vector3 originalScale;
        public Vector2 currentImageRectPos;
        float scaleDownAmount = 0.95f;
        bool isFadingIn = false;
        public Image parentImage;
        public Canvas parentCanvas;
        public CancellationTokenSource ImageCls = new CancellationTokenSource();
        public CancellationTokenSource buttonCls = new CancellationTokenSource();

        TweenFields tweenFields;
        public SelectableCardImage(Image iconImage,Button useButton,Button statusButton,Vector3 originalScale
            ,Canvas parentCanvas,Image parentImage)
        {
            this.iconImage = iconImage;
            this.useButton = useButton;
            this.statusButton = statusButton;
            this.originalScale = originalScale;
            this.parentCanvas = parentCanvas;
            this.parentImage = parentImage;

            tweenFields = new TweenFields(originalScale);
        }
        public async void OnPointerUp()
        {
            iconImage.rectTransform.SetParent(parentCanvas.transform);
            var scaleUpSet = tweenFields.simpleScaleUpSet;
            var scaleDownSet = tweenFields.simpleScaleDownSet;
            var scaleUpSet_2 = tweenFields.scaleUpWhenMoveSet;
            var moveSet = tweenFields.moveSet;
            Debug.Log(moveSet.endValue);
            try
            {
                await iconImage.gameObject.Scaler(scaleUpSet).ToUniTask(cancellationToken:ImageCls.Token);
                //await iconImage.gameObject.Scaler(scaleDownSet).ToUniTask(cancellationToken: cls.Token);
                var sequence = DOTween.Sequence();
                sequence.Append(iconImage.RectMover(moveSet))
                    .Join(iconImage.gameObject.Scaler(scaleUpSet_2));
                var task = sequence.ToUniTask(cancellationToken:ImageCls.Token);
                await task;
            }
            catch (OperationCanceledException)
            {
                SetOriginal();
            }
            //finally
            //{ 
            //    iconImage.rectTransform.localScale = originalScale; 
            //}
        }
            
        public void OnPointerDown() => iconImage.rectTransform.localScale = originalScale * scaleDownAmount;
        public async void OpenButtonUI()
        {
            var duration = tweenFields.buttonScaleDuration;
            var buttonScaleUp = tweenFields.buttonScaleUpSet;
            var task = useButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            var task2 = statusButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            await UniTask.WhenAll(task, task2);
        }

        public async void CloseButtonUI()
        {
            var duration = tweenFields.buttonScaleDuration;
            var buttonScaleDown = tweenFields.buttonScaleDownSet;
            var task = useButton.gameObject.Scaler(buttonScaleDown).ToUniTask();
            var task2 = statusButton.gameObject.Scaler(buttonScaleDown).ToUniTask();
            await UniTask.WhenAll(task, task2);
        }
        public void SetOriginal()
        {
            ImageCls?.Cancel();
            ImageCls?.Dispose();
            ImageCls = new CancellationTokenSource();
            Debug.Log("元に戻します");
            iconImage.transform.SetParent(parentImage.transform);
            var d = 0.2f;
            var originalPosSet = new Vector2TweenSetup(currentImageRectPos, d);
            var scaleSet = new Vector3TweenSetup(originalScale, d);
                 
            var moveTween =  iconImage.RectMover(originalPosSet);
            var scaleTween =  iconImage.gameObject.Scaler(scaleSet);
            var sequence = DOTween.Sequence();
            sequence.Append(moveTween)
                .Join(scaleTween);
        }
        public void FadeOutIconImage(CancellationTokenSource cls)
        {
            var set = tweenFields.fadeOutSet;
            var task = iconImage.Fader(set).ToUniTask(cancellationToken:cls.Token);

            //try
            //{
            //    await task;
            //}
            //catch (OperationCanceledException) 
            //{
            //    Debug.Log("Fadeがキャンセルされました");
            //    return;
            //}
            //iconImage.gameObject.SetActive(false);
        }
        public async void FadeInIconImage(CancellationTokenSource cls)
        {
            if (isFadingIn) return;
            isFadingIn = true;
            iconImage.gameObject.SetActive(true);
            var set = tweenFields.fadeInSet;
            var task = iconImage.Fader(set).ToUniTask(cancellationToken: cls.Token);
            try
            {
                await task;
            }
            catch (OperationCanceledException) { return; }
            finally { isFadingIn = false; }
        }
    }

    public CardData cardData  {get; set;} //{//ここにSelectCardManagerのCardDataを入れる
    ScrollRect scrollRect;
    public SelectableCardImage selectableCardImage { get; private set;}
  
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
   
    public void Initialize(ScrollRect scrollRect,UnityAction<bool> stopScrollAction,
        UnityAction<SelectableCard> selectedCardChanged,Canvas parentCanvas,Image parentImage)
    {
        this.scrollRect = scrollRect;
        this.SetCardImageFromData(cardData);
        var iconImage = GetComponent<Image>();
        var useButton = iconImage.transform.GetChild(1).GetComponent<Button>(); 
        var statusButton = iconImage.transform.GetChild(2).GetComponent<Button>();
        var originalScale = iconImage.rectTransform.localScale;
        selectableCardImage = new SelectableCardImage(
            iconImage,
            useButton,
            statusButton,
            originalScale,
            parentCanvas,
            parentImage
        );
        OnPointerDownStatus = stopScrollAction;
        OnSelectedCard = selectedCardChanged;

        selectableCardImage.currentImageRectPos = iconImage.rectTransform.anchoredPosition;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag終了");
        scrollRect.OnEndDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag中");
        scrollRect.OnDrag(eventData);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag開始");
        scrollRect.OnBeginDrag(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerDownStatus?.Invoke(false);
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
