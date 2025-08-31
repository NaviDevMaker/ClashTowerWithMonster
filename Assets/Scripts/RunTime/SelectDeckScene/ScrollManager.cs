using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SelectableCard;

public class ScrollManager : MonoBehaviour
{
    public static ScrollManager Instance { get; private set; } 
    ScrollRect scrollRect;

    bool isSliding = false;
    bool isStoping = false;
    CancellationTokenSource cls = new CancellationTokenSource();
    public CancellationTokenSource scrollCls { get; private set;} = new CancellationTokenSource();
    public bool isPointerDowned = false;
    EventTrigger eventTrigger;
    public SelectableCard currentSelectedCard {get;set;}

    private void Awake() => Instance = this;
    UnityAction fadeOutBattleButton;
    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log(Input.mouseScrollDelta.y);

        if (InputManager.IsClickedSlideButton())
        {
            if (!isSliding) StartSliding();
            else
            {
                cls.Cancel();
                cls.Dispose();
                cls = new CancellationTokenSource();
            }
        }
        else
        {
            if (isSliding) StopSliding();
            else scrollRect.vertical = false;
        }
    }
    public void Initialize(ScrollManagerActions scrollManagerActions)/*UnityAction setCameraPosToOriginal,UnityAction fadeInAction,
        UnityAction closeStatusUIAction,UnityAction fadeOutBattleButton,UnityAction transparentBattleButton*/
    {
        AddOnBeginDragEvent();

        var beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener((BaseEventData data) =>
        {
            scrollCls?.Cancel();
            scrollCls?.Dispose();
            scrollCls = new CancellationTokenSource();
            scrollManagerActions.setCameraPosToOriginal?.Invoke();
            scrollManagerActions.fadeInAction?.Invoke();
            scrollManagerActions.closeStatusUIAction?.Invoke();
            scrollManagerActions.transparentBattleButton?.Invoke();
            scrollManagerActions.enableLineRenderer?.Invoke();
            UnitManager.DestroyAll();
        });

        var endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        endDragEntry.callback.AddListener((BaseEventData data) =>
        {
            fadeOutBattleButton?.Invoke();
        });
        eventTrigger.triggers.Add(beginDragEntry);
        eventTrigger.triggers.Add(endDragEntry);
        this.fadeOutBattleButton = scrollManagerActions.fadeOutBattleButton;
    }
    void StartSliding()
    {
        cls.Cancel();
        cls.Dispose();
        cls = new CancellationTokenSource();
        isSliding = true;
        scrollRect.vertical = true;
    }

    async void StopSliding()
    {
        if (isStoping) return;
        Debug.Log("スライディングを止めます");
        try
        {
            isStoping = true;
            var interval = 1.0f;
            var time = 0f;
            while(time < interval && !cls.IsCancellationRequested)
            {
                time += Time.deltaTime;
                var y = Mathf.Abs(Input.mouseScrollDelta.y);
                if (y >= 1.0f || isPointerDowned)
                {
                    cls.Cancel();
                    break;
                }
                await UniTask.Yield(cancellationToken: cls.Token);
            }           
           
        }
        catch (OperationCanceledException) { }
        finally
        {
            scrollRect.vertical = false;
            isSliding = false;
            isStoping = false;
        }
    }
    void AddOnBeginDragEvent()
    {
        eventTrigger = GetComponent<EventTrigger>();
        if(eventTrigger == null) eventTrigger = this.gameObject.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener(SelectedCardSet);
        eventTrigger.triggers.Add(entry);
    }
    void SelectedCardSet(BaseEventData data)
    {
        if (currentSelectedCard == null) return;
        if (!currentSelectedCard.isSelectedDeck) currentSelectedCard.selectableCardImage.SetOriginal(isCalledScroll: true);
        if (currentSelectedCard.isSelectedDeck) currentSelectedCard.selectableCardImage.CloseRemoveButtonUI();
        currentSelectedCard = null;
    }
}
