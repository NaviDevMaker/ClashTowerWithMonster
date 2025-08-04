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
    public CancellationTokenSource cls { get; private set; } = new CancellationTokenSource();
    public bool isPointerDowned = false;
    EventTrigger eventTrigger;
    public SelectableCard currentSelectedCard {get;set;}
    public UnityAction OnScrolledImage;
    public UnityAction<CancellationTokenSource> FadeInAction;
    private void Awake() => Instance = this;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log(Input.mouseScrollDelta.y);
        if(InputManager.IsClickedSlideButton())
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
    public void Initialize(UnityAction<BaseEventData> setCameraPosToOriginal,UnityAction FadeInAction)
    {
        AddOnBeginDragEvent();

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.BeginDrag;
        entry.callback.AddListener((BaseEventData data) =>
        {
            setCameraPosToOriginal.Invoke(data);
            FadeInAction.Invoke();
        });

        eventTrigger.triggers.Add(entry);

        //var entry_FadeIn = new EventTrigger.Entry();
        //entry_FadeIn.eventID = EventTriggerType.BeginDrag;
        //entry_FadeIn.callback.AddListener((BaseEventData data) => FadeInAction.Invoke(cls));
        //eventTrigger.triggers.Add(entry_FadeIn);
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
        currentSelectedCard.selectableCardImage.SetOriginal();
        currentSelectedCard = null;
    }
}
