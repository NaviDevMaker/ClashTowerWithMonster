using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using System.IO;
using UnityEngine.EventSystems;

public class SelectableCardManager : MonoBehaviour
{
    public static SelectableCardManager Instance { get; private set; }

    class CardLineupInfo
    {
        public readonly float criterioX = -530f;
        public readonly float criterioY = 530f;
        public readonly float spaceX = 145f;
        public readonly float spaceY = -250f;

        public List<Vector2> cardPositionList = new List<Vector2>();
    }
    class CardDeckInfo
    {
        public readonly int deckColum = 4;
        public readonly int deckLine = 2;
        public readonly int deckCount = 8;
        public List<SelectableCard> deck;//これ後々ScriptableObjectで管理してGameManagerに渡す予定...やっぱそれ辞めできまったら送るようにしよう

        public CardDeckInfo() => deck = Enumerable.Repeat<SelectableCard>(null, deckCount).ToList();
    }

    public List<CardData> allCardDatas { get; set; } = new List<CardData>();
    [SerializeField] DeckChooseCameraMover deckChooseCameraMover;
    [SerializeField] DeckWarningText deckWarningText;   
    [SerializeField] Image parentImage;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Canvas parentCanvas;

    int line = 0;
    int columnCount = 0;
    GameObject cardImagePrefab;
    List<SelectableCard> selectableCards = new List<SelectableCard>();

    SelectableCard currentSelectedCard;
    CancellationTokenSource cls = new CancellationTokenSource();
    Func<SelectableCard, PrefabBase> OnSelectedCard;
    UnityAction<PrefabBase> UnableLineRenderer;
    CardLineupInfo cardLineupInfo = new CardLineupInfo();
    CardDeckInfo cardDeckInfo;
    DeckPreserver deckPreserver;
    UnityAction SetButtleButtonToOriginal;
    public readonly string fileName = "selectableCardData";

    class LineupActions
    {
       public Action<int, int> lineSetAction;
       public Action<MonsterStatusData, CancellationTokenSource> appearStatusUIAction;
       public Func<SelectableCard, (MonsterStatusData, SelectableMonster)> getStatusAndPrefabAction;
       public Func<SelectableCard, (SpellStatus, SelectableSpell)> getSpellStatusAndPrefabAction;
       public UnityAction setCameraPosAction;
       public UnityAction closeStatusUIAction;
    }

    private void Awake()
    {
        if (Instance != null) Instance = null;
        Instance = this;
    }

    private void Start()
    {
        Application.quitting += SaveDataToJson;
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //SaveDataToJson();
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.LogWarning("The file is deleted!!");
            }
        }
    }
    public async UniTask Initialize(CardManagerActions cardManagerActions)
        /*(Func<SelectableCard, PrefabBase> action, Action<int, int> lineSetAction,
        Action<MonsterStatusData, CancellationTokenSource> apperStatusUIAction,
        Func<SelectableCard, (MonsterStatusData, SelectableMonster)> getStatusAndPrefabAction,
        UnityAction setBattleButtonToOriginal, UnityAction cameraPositionSetAction,
        UnityAction closeStatusUIAction, UnityAction unableLineRenderer*/
    {
        cardDeckInfo = new CardDeckInfo();
        OnSelectedCard = cardManagerActions.onSelectedCardAction;
        SetButtleButtonToOriginal = cardManagerActions.setBattleButtonToOriginal;
        UnableLineRenderer = cardManagerActions.unableLineRenderer;
        var lineupActions = new LineupActions
        { 
            lineSetAction = cardManagerActions.lineSetAction,
            appearStatusUIAction = cardManagerActions.apperStatusUIAction,
            getStatusAndPrefabAction = cardManagerActions.getStatusAndPrefabAction,
            getSpellStatusAndPrefabAction = cardManagerActions.getSpellStatusAndPrefabAction,
            setCameraPosAction = cardManagerActions.cameraPositionSetAction,
            closeStatusUIAction = cardManagerActions.closeStatusUIAction,
        };

        await LineUpCards(lineupActions);/*lineSetAction,apperStatusUIAction,getStatusAndPrefabAction,
                          cameraPositionSetAction,closeStatusUIAction*/
        deckPreserver = await SetFieldFromAssets.SetField<DeckPreserver>("Datas/DeckPreserver");
        Debug.Log(deckPreserver);
        var savedCardData = LoadDataFromJson();
        if (savedCardData == null) return;
        LineupedBySavedData(savedCardData);
    }
    void SaveDataToJson()
    {
        var deck = cardDeckInfo.deck;
        // デッキの状態をデバッグ出力
        Debug.Log($"保存時のデッキ状態:");
        for (int i = 0; i < deck.Count; i++)
        {
            var card = deck[i];
            Debug.Log($"デッキ[{i}]: {(card != null ? $"sortOrder={card.sortOrder}" : "null")}");
        }
        var sortOrders = deck.Where(card => card != null).Select(card => card.sortOrder).ToList();
        Debug.Log(sortOrders.Count);
        var deckPlaceNumber = new List<int>();
        for (int i = 0; i < deck.Count; i++)
        {
            var card = deck[i];
            if(card != null) deckPlaceNumber.Add(i);
        }
        deckPlaceNumber.ForEach(n => Debug.Log(n));
        var deckData = new SaveCardData {
            sortOrders = sortOrders,
            deckPlaceNumber = deckPlaceNumber
        };
        string json = JsonUtility.ToJson(deckData);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath,fileName + ".json");
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"The data is saved to {filePath}");
    }
    SaveCardData LoadDataFromJson()
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".json");
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Not founed file path!!");
            return null;
        }
        string json = File.ReadAllText(filePath);
        SaveCardData data = JsonUtility.FromJson<SaveCardData>(json);
        return data;
    }

    void LineupedBySavedData(SaveCardData savedCardData)
    {
        var sortOrders = savedCardData.sortOrders;
     
        var placeNumber = savedCardData.deckPlaceNumber;
        for (int i = 0; i < placeNumber.Count; i++)
        {
            var deckIndex = placeNumber[i];
            var sortOrder = sortOrders[i];
            var card = selectableCards.FirstOrDefault(card => card.sortOrder == sortOrder);
            if (card != null)
            {
                cardDeckInfo.deck[deckIndex] = card;
                Debug.Log($"カード発見: sortOrder={card.sortOrder}, lineupIndex={card.lineupIndex}");
            }
        }

        var deck = cardDeckInfo.deck;
        Debug.Log($"デッキのカウントは{deck.Count}");
        deck.ForEach(x => Debug.Log(x));

        var line = cardDeckInfo.deckLine;
        var column = cardDeckInfo.deckColum;
        Action<SelectableCard, int> moveToDeckAction = ((card, index) =>
        {
            card.selectableCardImage.SetCardToDeck(index, column, line);
            card.DeckSelectedStateChange(true);
        });

        for (int i = 0; i < deck.Count; i++)
        {
            var card = deck[i];
            if (card == null) continue;
            moveToDeckAction.Invoke(card, i);
        }
        var reminingCards = GetLineupedList();
        for (int i = 0; i < reminingCards.Count; i++)
        {
            var card = reminingCards[i];
            var pos = cardLineupInfo.cardPositionList[i];
            card.lineupIndex = i;
            card.selectableCardImage.SetCardToPool(pos);
        }
    }
    async UniTask LineUpCards(LineupActions lineupActions)/*Action<int, int> lineSetAction,Action<MonsterStatusData,CancellationTokenSource> appearStatusUIAction,
        Func<SelectableCard,(MonsterStatusData,SelectableMonster)> getStatusAndPrefabAction,UnityAction setCameraPosAction,
        UnityAction closeStatusUIAction*/
        {
        var cardDatas = (await SetFieldFromAssets.SetFieldByLabel<CardData>("CardData")).ToList();
        allCardDatas = cardDatas.OrderBy(data => data.SortOrder).ToList();
        cardImagePrefab = await SetFieldFromAssets.SetField<GameObject>("Prefabs/SelectableCard");
        if (cardImagePrefab == null) return;
        var criterioX = cardLineupInfo.criterioX;
        var criterioY = cardLineupInfo.criterioY;
        var spaceX = cardLineupInfo.spaceX;
        var spaceY = cardLineupInfo.spaceY;
        var dataCount = allCardDatas.Count;
        columnCount = 5;
        line = Mathf.CeilToInt((float)dataCount / columnCount);
        Debug.Log($"Lineは{line},コラムは{columnCount}");
        lineupActions.lineSetAction.Invoke(line, columnCount);
        var instanciatedCount = 0;
        var index = 0;

        UnityAction<bool> stopAction = (isDowned) => ScrollManager.Instance.isPointerDowned = isDowned;
        var cardActions = new CardActions
        { 
            stopScrollAction = stopAction,
            selectedCardChanged = OnSelectedCardChanged,
            selectedCardtoDeck = OnCardSelectedToDeck,
            selectedFromDeck = OnSelectedCardFromDeck,
            removedFromDeck = OnRemovedFromDeck,
            appearStatusUIAction = lineupActions.appearStatusUIAction,
            getStatusAndPrefabAction = lineupActions.getStatusAndPrefabAction,
            getSpellStatusAndPrefabAction = lineupActions.getSpellStatusAndPrefabAction,
            setCameraPosAction = lineupActions.setCameraPosAction,
            closeStatusUIAction = lineupActions.closeStatusUIAction,
        };

        for (int l = 0; l < line; l++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                var pos = new Vector2(criterioX + spaceX * c, criterioY + l * spaceY);
                cardLineupInfo.cardPositionList.Add(pos);
                var obj = Instantiate(cardImagePrefab);
                obj.transform.SetParent(parentImage.transform);
                var image = obj.GetComponent<Image>();
                if (image != null) image.rectTransform.localPosition = pos;
                var cmp = obj.GetComponent<SelectableCard>();
                if (cmp != null)
                {
                    cmp.cardData = allCardDatas[index];
                    cmp.lineupIndex = index;
                    selectableCards.Add(cmp);
                    cmp.Initialize(scrollRect,cardActions,parentCanvas, parentImage);/* stopAction, OnSelectedCardChanged, OnCardSelectedToDeck, OnSelectedCardFromDeck,
                        OnRemovedFromDeck,appearStatusUIAction,getStatusAndPrefabAction,setCameraPosAction,closeStatusUIAction*/
                    instanciatedCount++;
                    if (instanciatedCount == dataCount) break;
                    index++;
                }
            }
        }
    }

    void CardFadeOut(CancellationTokenSource scrollCls)
    {
        selectableCards.ForEach(card =>
        {
            if (card._isSelected) return;// || card.isSelectedDeck
            card.selectableCardImage.FadeOutIUI(scrollCls);
        });
    }
    public void CardFadeIn()
    {
        selectableCards.ForEach(card =>
        {
            card.selectableCardImage.FadeInIconUI(cls);
        });
    }
    void OnCardSelectedToDeck(SelectableCard selectedCard)
    {
        if (selectedCard == null) return;
        var cardIndex = selectedCard.lineupIndex;
        var column = cardDeckInfo.deckColum;
        var line = cardDeckInfo.deckLine;
        var index = cardDeckInfo.deck.FindIndex(card => card == null);
        if (index == -1) return;

        selectedCard.selectableCardImage.SetCardToDeck(index, column, line);
        cardDeckInfo.deck[index] = selectedCard;

        LineUpToPool(cardIndex);
    }
    void LineUpToPool(int lineupIndex)
    {
        var lineupedList = GetLineupedList();
        for (int i = lineupIndex + 1; i < lineupedList.Count; i++)
        {
            var targetCard = lineupedList[i];
            var positionIndex = i - 1;
            var pos = cardLineupInfo.cardPositionList[positionIndex];
            targetCard.lineupIndex = positionIndex;
            targetCard.selectableCardImage.SetCardToPool(pos);
        }
        cardDeckInfo.deck.ForEach(x => Debug.Log(x));
    }
    void OnRemovedFromDeck(SelectableCard removedCard)
    {
        ScrollManager.Instance.currentSelectedCard = null;
        var deckIndex = cardDeckInfo.deck.FindIndex(card => card == removedCard);
        if (deckIndex == -1) return;
        cardDeckInfo.deck[deckIndex] = null;
        var removedCardOrder = removedCard.sortOrder;
        var frontIndexCard = selectableCards.Where(card =>
            {
                var isFront = removedCardOrder > card.sortOrder;
                var isNotDeck = !card.isSelectedDeck;
                return isFront && isNotDeck;
            }).LastOrDefault();
        var startLineupIndex = frontIndexCard != null ? frontIndexCard.lineupIndex + 1 : 0;
        var lineupedList = GetLineupedList();

        removedCard.lineupIndex = startLineupIndex;
        var removedCardPos = cardLineupInfo.cardPositionList[startLineupIndex];
        removedCard.selectableCardImage.SetCardToPool(removedCardPos);

        for (var i = startLineupIndex; i < lineupedList.Count; i++)
        {
            var targetCard = lineupedList[i];
            var positionIndex = i + 1;
            var pos = cardLineupInfo.cardPositionList[positionIndex];
            targetCard.lineupIndex = positionIndex;
            targetCard.selectableCardImage.SetCardToPool(pos);
        }
    }
    List<SelectableCard> GetLineupedList()
    {
        var lineupedList = selectableCards
            .Where(card => !card.isSelectedDeck)
            .OrderBy(card => card.lineupIndex).ToList();
        return lineupedList;
    }
    async void OnSelectedCardChanged(SelectableCard selectedCard)
    {
        SetButtleButtonToOriginal?.Invoke();
        var sameCard = currentSelectedCard == selectedCard;
        cls?.Cancel();
        cls?.Dispose();
        if (currentSelectedCard != null && !sameCard)
        {
            if (!currentSelectedCard.isSelectedDeck) currentSelectedCard.selectableCardImage.SetOriginal();
            currentSelectedCard._isSelected = false;

            var previousPrefab = deckChooseCameraMover.currentSelectedPrefab;

            if (!currentSelectedCard.isSelectedDeck)
            {
                if (previousPrefab is ISelectableMonster monster)
                {
                    monster.expectedCls = null;
                    monster.Repetrification();
                }
            }
            deckChooseCameraMover.currentSelectedPrefab = null;
        }
        cls = new CancellationTokenSource();
        var scrollCls = ScrollManager.Instance.scrollCls;
        CardFadeOut(scrollCls);
        currentSelectedCard = selectedCard;
        ScrollManager.Instance.currentSelectedCard = selectedCard;
        selectedCard.selectableCardImage.FadeCancelAction();
        deckChooseCameraMover.selectedCardCls = cls;
        deckChooseCameraMover.currentSelectedPrefab = OnSelectedCard?.Invoke(selectedCard);
        UnableLineRenderer?.Invoke(deckChooseCameraMover.currentSelectedPrefab);
        await deckChooseCameraMover.MoveToFrontOfObj();
        var currentPrefab = deckChooseCameraMover.currentSelectedPrefab;
        Func<bool> GetSetOriginalPos = (() => deckChooseCameraMover.isSettedOriginalPos);
        if (currentPrefab is ISelectableMonster selectableMonster)
        {
            selectableMonster.expectedCls = cls;
            selectableMonster.Depetrification(cls, GetSetOriginalPos);
        }
        else if(currentPrefab is ISelectableSpell selectableSpell)
        {
            
        }
    }
    void OnSelectedCardFromDeck(SelectableCard selectedCard)
    {       
        cls?.Cancel();
        cls?.Dispose();

        var previousPrefab = deckChooseCameraMover.currentSelectedPrefab;
        if (previousPrefab != null && !currentSelectedCard.isSelectedDeck)
        {
            if (previousPrefab is ISelectableMonster monster)
            {
                monster.expectedCls = null;
            }
        }

        cls = new CancellationTokenSource();
        selectableCards.ForEach(card => card.selectableCardImage.FadeCancelAction());
        ///これ順番大事、ぐちゃぐちゃになってるすまん俺
        deckChooseCameraMover.selectedCardCls = cls;
        deckChooseCameraMover.SetOriginalPos();
        deckChooseCameraMover.currentSelectedPrefab = null;
        var previousSelectedCard = currentSelectedCard;
        currentSelectedCard = selectedCard;
        var sameCard = selectedCard == previousSelectedCard;
        if(previousSelectedCard != null && !sameCard)
        {
            previousSelectedCard._isSelected = false;
           if(!previousSelectedCard.isSelectedDeck) previousSelectedCard.selectableCardImage.SetOriginal();
        }
        ScrollManager.Instance.currentSelectedCard = selectedCard;
    }

    public bool GetChoosenDeckDatas()
    {
        List<CardData> deckCardDatas = Enumerable.Repeat<CardData>(null, cardDeckInfo.deckCount).ToList();
        for (int i = 0; i < deckCardDatas.Count; i++)
        {
            var c = cardDeckInfo.deck[i];
            if (c == null) continue;
            var d = c.cardData;
            deckCardDatas[i] = d;
        }
        if(deckCardDatas.Contains(null))
        {
            deckWarningText.AppearWarningText(cls);
            return false;
        }
        else
        {
            deckPreserver.ChoosenDecks = deckCardDatas;
            return true;
        }
    }

   public CancellationTokenSource GetClickedCancellationTokenSource() => cls;
   public List<SelectableCard> GetDeck() => cardDeckInfo.deck;
}
