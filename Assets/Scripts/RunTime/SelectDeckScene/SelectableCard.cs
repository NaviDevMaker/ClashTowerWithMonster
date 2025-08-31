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
            readonly float buttonScale = 0.75f;
            public readonly float simpleScaleDuration = 0.075f;
            public readonly float buttonScaleDuration = 0.1f;
            public readonly float moveDuration = 1.0f;
            readonly Vector2 moveTarget = new Vector2(275f,-120f);
            readonly Vector2 useButtonMoveTarget = new Vector2(-5,-220);
            readonly Vector2 statusButtonMoveTarget = new Vector2(0, -170);
            public Vector3 endValue { get; private set; }
            public Vector3 endValue_InDeck { get; private set; }
            public Vector3TweenSetup simpleScaleUpSet { get; private set; }
            public Vector3TweenSetup simpleScaleDownSet { get; private set; }

            public Vector3TweenSetup buttonScaleUpSet {  get; private set; }
            public Vector3TweenSetup buttonScaleDownSet { get; private set; }
            public Vector3TweenSetup buttonScaleDown_Move {get; private set; }
            public Vector3TweenSetup scaleUpWhenMoveSet { get; private set; }

            public Vector3TweenSetup scaleUpWhenInDeck { get; private set; }
            public Vector2TweenSetup moveSet {  get; private set; }
            public Vector2TweenSetup buttonMoveSet_Use {  get; private set; }
            public Vector2TweenSetup buttonMoveSet_Status { get; private set; }
            public FadeSet fadeOutSet { get; private set; }
            public FadeSet fadeInSet { get; private set; }
            public TweenFields(Vector3 originalScale,Vector3 inDeckOriginalScale)
            {
                endValue = originalScale * scaleUpAmount;
                endValue_InDeck = inDeckOriginalScale * scaleUpAmount;
                simpleScaleUpSet = new Vector3TweenSetup(endValue, simpleScaleDuration);
                simpleScaleDownSet = new Vector3TweenSetup(originalScale, simpleScaleDuration);
                buttonScaleUpSet = new Vector3TweenSetup(Vector3.one, buttonScaleDuration,Ease.Linear);
                buttonScaleDownSet = new Vector3TweenSetup(Vector3.zero, buttonScaleDuration,Ease.Linear);
                buttonScaleDown_Move = new Vector3TweenSetup(Vector3.one * buttonScale, moveDuration);
                scaleUpWhenMoveSet = new Vector3TweenSetup(Vector3.one, moveDuration);
                scaleUpWhenInDeck = new Vector3TweenSetup(endValue_InDeck,simpleScaleDuration);
                moveSet = new Vector2TweenSetup(moveTarget,moveDuration);
                buttonMoveSet_Use = new Vector2TweenSetup(useButtonMoveTarget,moveDuration);
                buttonMoveSet_Status = new Vector2TweenSetup(statusButtonMoveTarget,moveDuration);
                fadeOutSet = new FadeSet(0f,moveDuration);
                fadeInSet = new FadeSet(1f,moveDuration);
            }
        }

        public Image iconImage;
        Image energyImage;
        public Image useButtonImage;
        public Image statusButtonImage;
        Button useButton;
        Button statusButton;
        Button removeButton;
        Vector3 originalScale;
        Vector3 inDeckOriginalScale;
        public Vector2 currentImageRectPos;
        float scaleDownAmount = 0.95f;

        float originalHeight_Use;
        float originalHeight_Status;
        public Vector2 currentUseButtonPos;
        public Vector2 currentStatusButtonPos;

        bool isFadingIn = false;
        public Image parentImage;
        public Canvas parentCanvas;
        public CancellationTokenSource scaleCls = new CancellationTokenSource();
        public CancellationTokenSource fadeCls  = new CancellationTokenSource();   
        public CancellationTokenSource doubleCls  = new CancellationTokenSource();
        public CancellationTokenSource buttonCls = new CancellationTokenSource();

        TweenFields tweenFields;

        //Use 60 Vector2(-5,-220) Status 70 Vector2(0,-170)
        public SelectableCardImage(Image iconImage,Image energyImage,Button useButton,Button statusButton,Button removeButton,
            Vector3 originalScale,Canvas parentCanvas,Image parentImage)
        {
            this.iconImage = iconImage;
            this.energyImage = energyImage;
            this.useButton = useButton;
            this.statusButton = statusButton;
            this.removeButton = removeButton;
            this.originalScale = originalScale;
            this.parentCanvas = parentCanvas;
            this.parentImage = parentImage;

            inDeckOriginalScale = Vector3.one * 0.8f;
            useButtonImage = useButton.GetComponent<Image>();
            statusButtonImage = statusButton.GetComponent<Image>();
            originalHeight_Use = useButtonImage.rectTransform.rect.height;
            originalHeight_Status = statusButtonImage.rectTransform.rect.height;
            tweenFields = new TweenFields(originalScale,inDeckOriginalScale);
        }
        public async void OnPointerUp()
        {
            iconImage.rectTransform.SetParent(parentCanvas.transform);
            var scaleUpSet = tweenFields.simpleScaleUpSet;
            var scaleDownSet = tweenFields.simpleScaleDownSet;
            var scaleUpSet_2 = tweenFields.scaleUpWhenMoveSet;
            var buttonScale = tweenFields.buttonScaleDown_Move;
            var moveSet = tweenFields.moveSet;
            var useButtonMoveSet = tweenFields.buttonMoveSet_Use;
            var statusButtonMoveSet = tweenFields.buttonMoveSet_Status;
            var newHeight_Use = 60f;
            var newHeight_Status = 70f;
            Debug.Log(moveSet.endValue);
            try
            {
                await iconImage.gameObject.Scaler(scaleUpSet).ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait
                    , cancellationToken:scaleCls.Token);

                HeightSet(useButtonImage, newHeight_Use);
                HeightSet(statusButtonImage, newHeight_Status);
                var sequence = DOTween.Sequence();
                sequence.Append(iconImage.RectMover(moveSet))
                    .Join(iconImage.gameObject.Scaler(scaleUpSet_2))
                    .Join(useButton.gameObject.Scaler(buttonScale))
                    .Join(statusButton.gameObject.Scaler(buttonScale))
                    .Join(useButtonImage.RectMover(useButtonMoveSet))
                    .Join(statusButtonImage.RectMover(statusButtonMoveSet));
                var task = sequence.ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait,cancellationToken:scaleCls.Token);
                await task;
            }
            catch (OperationCanceledException)
            {
                SetOriginal();
            }
        }
        public async void OnPointerUpInDeck()
        {
            iconImage.transform.SetAsLastSibling();
            var d = 0.2f;
            var scaleSet = tweenFields.scaleUpWhenInDeck;
            var originalScaleSet = new Vector3TweenSetup(inDeckOriginalScale, d);
            var sequence = DOTween.Sequence();
            sequence.Append(iconImage.gameObject.Scaler(scaleSet))
                .Join(iconImage.gameObject.Scaler(originalScaleSet));
                
            var task = sequence.ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait,cancellationToken: scaleCls.Token);
            try { await task; }
            catch (OperationCanceledException) { }
            finally{ iconImage.transform.localScale = inDeckOriginalScale;}          
        }
        public void OnPointerDown(bool isDeck)
        {
            if (!isDeck) iconImage.rectTransform.localScale = originalScale * scaleDownAmount;
            else iconImage.rectTransform.localScale = inDeckOriginalScale * scaleDownAmount;
        }
        public async void OpenUseAndStatusButtonUI()
        {
            var duration = tweenFields.buttonScaleDuration;
            var buttonScaleUp = tweenFields.buttonScaleUpSet;
            var task = useButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            var task2 = statusButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            await UniTask.WhenAll(task, task2);
        }
        public async void OpenRemoveButtonUI()
        {
            var duration = tweenFields.buttonScaleDuration;
            var buttonScaleUp = tweenFields.buttonScaleUpSet;
            var task =  removeButton.gameObject.Scaler(buttonScaleUp).ToUniTask();
            await task;
        }
        public async void CloseUseAndStatusButtonUI()
        {
            Debug.Log("ボタンUIを閉じます");
            var duration = tweenFields.buttonScaleDuration;
            var buttonScaleDown = tweenFields.buttonScaleDownSet;
            var task = useButton.gameObject.Scaler(buttonScaleDown).ToUniTask();
            var task2 = statusButton.gameObject.Scaler(buttonScaleDown).ToUniTask();
            await UniTask.WhenAll(task, task2);
        }
        public async void CloseRemoveButtonUI()
        {
            var duration = tweenFields.buttonScaleDuration;
            var buttonScaleDownSet = tweenFields.buttonScaleDownSet;
            var task = removeButton.gameObject.Scaler(buttonScaleDownSet).ToUniTask();
            await task;
        }
        public void SetOriginal(bool isCalledScroll = false)
        {
            scaleCls?.Cancel();
            scaleCls?.Dispose();
            scaleCls = new CancellationTokenSource();

            Debug.Log("元に戻します");
            iconImage.transform.SetParent(parentImage.transform);
            HeightSet(useButtonImage,originalHeight_Use);
            HeightSet(statusButtonImage, originalHeight_Status);

            var d = 0.2f;
            var originalPosSet = new Vector2TweenSetup(currentImageRectPos, d);
            var scaleSet = new Vector3TweenSetup(originalScale, d);
            //var useButtonMoveSet = new Vector2TweenSetup(currentUseButtonPos, d);
            //var statusButtonMoveSet = new Vector2TweenSetup(currentStatusButtonPos, d);
            var moveTween =  iconImage.RectMover(originalPosSet);
            var scaleTween =  iconImage.gameObject.Scaler(scaleSet);
            var parentSequence = DOTween.Sequence();
            var buttonSequence = GetButtonMoveSequence(d);
            parentSequence.Append(moveTween)
                .Join(scaleTween)
                .Join(buttonSequence);
          
            if (isCalledScroll)
            {
                var buttonScaleSet = new Vector3TweenSetup(Vector3.one, d);
                var childSequence = DOTween.Sequence();
                childSequence.Join(useButton.gameObject.Scaler(buttonScaleSet))
               .Join(statusButton.gameObject.Scaler(buttonScaleSet));
                parentSequence.Join(childSequence);
            }
        }

        Sequence GetButtonMoveSequence(float duration)
        {
            var useButtonMoveSet = new Vector2TweenSetup(currentUseButtonPos, duration);
            var statusButtonMoveSet = new Vector2TweenSetup(currentStatusButtonPos,duration);
            var sequence = DOTween.Sequence();
            sequence.Append(useButtonImage.RectMover(useButtonMoveSet))
                .Join(statusButtonImage.RectMover(statusButtonMoveSet));
            return sequence;
        }
        public async void FadeOutIUI(CancellationTokenSource cls)
        {
            var set = tweenFields.fadeOutSet;
            doubleCls = CancellationTokenSource.CreateLinkedTokenSource(cls.Token,fadeCls.Token);
            var task = iconImage.Fader(set).ToUniTask(tweenCancelBehaviour:TweenCancelBehaviour.KillAndCancelAwait,cancellationToken: doubleCls.Token);
            var task2 = energyImage.Fader(set).ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait,cancellationToken: doubleCls.Token);

            try
            {
                await UniTask.WhenAll(task,task2);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            if(!cls.IsCancellationRequested && !fadeCls.IsCancellationRequested) iconImage.gameObject.SetActive(false);
        }
        public async void FadeInIconUI(CancellationTokenSource cls)
        {
            if (isFadingIn) return;
            isFadingIn = true;
            iconImage.gameObject.SetActive(true);
            var set = tweenFields.fadeInSet;
            var task = iconImage.Fader(set).ToUniTask(cancellationToken: cls.Token);
            var task2 = energyImage.Fader(set).ToUniTask(cancellationToken: cls.Token);
            try
            {
                await UniTask.WhenAll(task,task2);
            }
            //catch (OperationCanceledException) { return; }
            finally { isFadingIn = false; }
        }
        public void FadeCancelAction()
        {
            Debug.Log("Fadeがキャンセルされました");
            fadeCls?.Cancel();
            fadeCls?.Dispose();
            doubleCls?.Dispose();
            fadeCls = new CancellationTokenSource();

            var iconColor = iconImage.color;
            var energyColor = energyImage.color;
            iconColor.a = 1.0f;
            energyColor.a = 1.0f;
            iconImage.color = iconColor;
            energyImage.color = energyColor;
        }
        public void SetCardToDeck(int index,int deckColumCount,int deckLine)
        {
            HeightSet(useButtonImage, originalHeight_Use);
            HeightSet(statusButtonImage, originalHeight_Status);

            var duration = 0.01f;
            var buttonScaleMoveTween = GetButtonMoveSequence(duration);
            CloseUseAndStatusButtonUI();
            iconImage.transform.SetParent(parentImage.transform);
            //-540,950 差は200と-230 scaleは0.8
            iconImage.transform.localScale = inDeckOriginalScale;
            var x = -540f;
            var y = 950f;
            var spaceX = 200;
            var spaceY = -230;
            var line = index / deckColumCount;
            var column = index % deckColumCount;
          
            var pos = new Vector2(x + spaceX * column, y + spaceY * line);
            iconImage.rectTransform.localPosition = pos;
        }
        public void SetCardToPool(Vector2 pos)
        {
            iconImage.transform.localScale = originalScale;
           
            CloseRemoveButtonUI();
            iconImage.rectTransform.anchoredPosition = pos;
            SetCurrentPos();
        }
        void HeightSet(Image targetImage,float targetHeight)
        {
            var newSize = targetImage.rectTransform.sizeDelta;
            newSize.y = targetHeight;
            targetImage.rectTransform.sizeDelta = newSize;  
        }
        public void SetCurrentPos()
        {
            currentImageRectPos = iconImage.rectTransform.anchoredPosition;
            currentUseButtonPos = useButtonImage.rectTransform.anchoredPosition;
            currentStatusButtonPos = statusButtonImage.rectTransform.anchoredPosition;
        }
    }
    public CardData cardData  {get; set;} //{//ここにSelectCardManagerのCardDataを入れる
    ScrollRect scrollRect;
    public SelectableCardImage selectableCardImage { get; private set;}
    public int sortOrder = 0;
    public int lineupIndex = 0;// { get; set; }
    bool IsSelected = false;
    public bool isSelectedDeck  = false;//このFlagは単にこのカードがDeckのなかに入ってるかどうかのこと{ get;private set;}
    public bool _isSelected 
    { 
        get => IsSelected; 
        set
        {
            if (value == IsSelected) return;
            IsSelected = value;
            if (!IsSelected)
            {
                selectableCardImage?.CloseRemoveButtonUI();
                selectableCardImage?.CloseUseAndStatusButtonUI();
            }
            else selectableCardImage?.scaleCls?.Cancel();
        }
    } 

    public UnityAction<bool> OnPointerDownStatus;
    public UnityAction<SelectableCard> OnSelectedCard;
    UnityAction<SelectableCard> OnSelectedCardFromDeck;

    public CancellationTokenSource statusButtonCls = new CancellationTokenSource();
    CancellationTokenSource useButtonCls = new CancellationTokenSource();
    public CancellationTokenSource removedButtonCls { get; private set;} = new CancellationTokenSource();
    public void Initialize(ScrollRect scrollRect,CardActions cardActions,Canvas parentCanvas,Image parentImage)/*UnityAction<bool> stopScrollAction,
        UnityAction<SelectableCard> selectedCardChanged,UnityAction<SelectableCard> selectedCardtoDeck,
        UnityAction<SelectableCard> selectedFromDeck,UnityAction<SelectableCard> removedFromDeck,
        Action<MonsterStatusData,CancellationTokenSource> appearStatusUIAction,
        Func<SelectableCard,(MonsterStatusData data,SelectableMonster prefab)> getStatusAndPrefabAction, 
        UnityAction setCameraPosAction,UnityAction closeStatusUIAction*/
    {
        this.scrollRect = scrollRect;
        this.SetCardImageFromData(cardData);
        var iconImage = GetComponent<Image>();
        var energyImage = iconImage.transform.GetChild(0).GetComponent<Image>();
        var useButton = iconImage.transform.GetChild(1).GetComponent<Button>(); 
        var statusButton = iconImage.transform.GetChild(2).GetComponent<Button>();
        var removeButton = iconImage.transform.GetChild(3).GetComponent<Button>();
        var originalScale = iconImage.rectTransform.localScale;
        selectableCardImage = new SelectableCardImage(
            iconImage,
            energyImage,
            useButton,
            statusButton,
            removeButton,
            originalScale,
            parentCanvas,
            parentImage
        );
        OnPointerDownStatus = cardActions.stopScrollAction;
        OnSelectedCard = cardActions.selectedCardChanged;
        OnSelectedCardFromDeck = cardActions.selectedFromDeck;

        selectableCardImage.SetCurrentPos();
        useButton.onClick.AddListener(() =>
        {
            UnitManager.DestroyAll();
            cardActions.closeStatusUIAction.Invoke();
            useButtonCls?.Cancel();
            useButtonCls?.Dispose();
            useButtonCls = new CancellationTokenSource();
            cardActions.setCameraPosAction.Invoke();
            var prefab = cardData.CardType switch
            {
                CardType.Monster => cardActions.getStatusAndPrefabAction(this).prefab,
                CardType.Spell => cardActions.getSpellStatusAndPrefabAction(this).prefab,
                _ => default(PrefabBase)
            };
            prefab.SetSelectedEffect(removedButtonCls);
            cardActions.selectedCardtoDeck.Invoke(this);
            cardActions.enableLineRenderer.Invoke();
            DeckSelectedStateChange(true);
        });

        statusButton.onClick.AddListener(() =>
        {
            UnitManager.DestroyAll();
            statusButtonCls = new CancellationTokenSource();

            var scrollCls = ScrollManager.Instance.scrollCls;
            var doubleCls = CancellationTokenSource.CreateLinkedTokenSource(scrollCls.Token, useButtonCls.Token);//statusButtonCls
            if (cardData.CardType == CardType.Monster)
            {
                var statusData = cardActions.getStatusAndPrefabAction(this).data;
                var monsterPrefab = cardActions.getStatusAndPrefabAction(this).prefab;
                if (statusData == null)
                {
                    Debug.LogWarning("The data don't exist!!");
                    return;
                }
                var motionCls = CancellationTokenSource.CreateLinkedTokenSource(scrollCls.Token, useButtonCls.Token);
                cardActions.appearStatusUIAction.Invoke(statusData, doubleCls);
                monsterPrefab.attackMotionPlay(monsterPrefab, motionCls);
            }
            else if(cardData.CardType == CardType.Spell)
            {
                var statusData = cardActions.getSpellStatusAndPrefabAction(this).data;
                var spellPrefab = cardActions.getSpellStatusAndPrefabAction(this).prefab;
                if (statusData == null)
                {
                    Debug.LogWarning("The data don't exist!!");
                    return;
                }
                cardActions.appearStatusUIAction.Invoke(statusData, doubleCls);
                spellPrefab.SpellInvoke(doubleCls);
            }        
        });

        removeButton.onClick.AddListener(() =>
        {
            removedButtonCls?.Cancel();
            removedButtonCls?.Dispose();
            removedButtonCls = new CancellationTokenSource();
            cardActions.removedFromDeck.Invoke(this);
            IsSelected = false;
            DeckSelectedStateChange(false);
            var prefab = cardData.CardType switch
            {
                CardType.Monster => cardActions.getStatusAndPrefabAction(this).prefab,
                CardType.Spell => cardActions.getSpellStatusAndPrefabAction(this).prefab,
                _=> default(PrefabBase)
            };
            prefab.ScalerToZero();
        });
        sortOrder = cardData.SortOrder;
    }
    public void DeckSelectedStateChange(bool isSelectedDeck) => this.isSelectedDeck = isSelectedDeck;
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
    public void OnPointerExit(PointerEventData eventData) => OnPointerDownStatus?.Invoke(false);
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPointerDownStatus?.Invoke(true);
        selectableCardImage?.OnPointerDown(isSelectedDeck);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        IsSelected = true;

        if (!isSelectedDeck)
        {
            selectableCardImage?.OnPointerUp();
            selectableCardImage?.OpenUseAndStatusButtonUI();
            OnSelectedCard?.Invoke(this);
        }
        else
        {
            selectableCardImage?.OpenRemoveButtonUI();
            selectableCardImage?.OnPointerUpInDeck();
            OnSelectedCardFromDeck?.Invoke(this);
        }
    }

}

public class CardActions
{
   public UnityAction<bool> stopScrollAction;
   public UnityAction<SelectableCard> selectedCardChanged;
   public UnityAction<SelectableCard> selectedCardtoDeck;
   public UnityAction<SelectableCard> selectedFromDeck;
   public UnityAction<SelectableCard> removedFromDeck;
   public Action<ScriptableObject, CancellationTokenSource> appearStatusUIAction;
   public Func<SelectableCard, (MonsterStatusData data, SelectableMonster prefab)> getStatusAndPrefabAction;
   public Func<SelectableCard, (SpellStatus data, SelectableSpell prefab)> getSpellStatusAndPrefabAction;
   public UnityAction setCameraPosAction;
   public UnityAction closeStatusUIAction;
   public UnityAction enableLineRenderer;
}
