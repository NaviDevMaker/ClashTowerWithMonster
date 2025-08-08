using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System.Threading;
using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

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
    CardLineupInfo cardLineupInfo = new CardLineupInfo();
    CardDeckInfo cardDeckInfo;
    DeckPreserver deckPreserver;
    private void Awake()
    {
        if (Instance != null) Instance = null;
        Instance = this;
    }

    private void Update()//テスト用
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            List<CardData> deckCardDatas = Enumerable.Repeat<CardData>(null,cardDeckInfo.deckCount).ToList();
            for (int i = 0; i < deckCardDatas.Count; i++)
            {
                var c = cardDeckInfo.deck[i];
                if (c == null) continue;
                var d = c.cardData;
                deckCardDatas[i] = d;
            }
            deckPreserver.ChoosenDecks = deckCardDatas;
        }
    }
    public async UniTask Initialize(Func<SelectableCard, PrefabBase> action, Action<int, int> lineSetAction)
    {
        cardDeckInfo = new CardDeckInfo();
        OnSelectedCard = action;
        await LineUpCards(lineSetAction);
        deckPreserver = await SetFieldFromAssets.SetField<DeckPreserver>("Datas/DeckPreserver");
        Debug.Log(deckPreserver);
    }
    async UniTask LineUpCards(Action<int, int> lineSetAction)
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
        lineSetAction.Invoke(line, columnCount);
        var instanciatedCount = 0;
        var index = 0;
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
                    UnityAction<bool> stopAction = (isDowned) => ScrollManager.Instance.isPointerDowned = isDowned;
                    cmp.Initialize(scrollRect, stopAction, OnSelectedCardChanged, OnCardSelectedToDeck, OnSelectedCardFromDeck,
                        OnRemovedFromDeck,parentCanvas, parentImage);
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
            if (card._isSelected || card.isSelectedDeck) return;
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
        var cardIndex = selectedCard.lineupIndex;
        var column = cardDeckInfo.deckColum;
        var line = cardDeckInfo.deckLine;
        var index = cardDeckInfo.deck.FindIndex(card => card == null);
        if (index == -1) return;

        selectedCard.selectableCardImage.SetCardToDeck(index, column, line);
        cardDeckInfo.deck[index] = selectedCard;
        var lineupedList = GetLineupedList();
        for (int i = cardIndex + 1; i < lineupedList.Count; i++)
        {
            var targetCard = lineupedList[i];
            var positionIndex = i - 1;
            var pos = cardLineupInfo.cardPositionList[positionIndex];
            targetCard.lineupIndex = positionIndex;
            targetCard.selectableCardImage.SetCardToPool(pos);
        }
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
        var sameCard = currentSelectedCard == selectedCard;
        if (currentSelectedCard != null && !sameCard)
        {
            cls?.Cancel();
            cls?.Dispose();

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
        await deckChooseCameraMover.MoveToFrontOfObj();
        var currentPrefab = deckChooseCameraMover.currentSelectedPrefab;
        Func<bool> GetSetOriginalPos = (() => deckChooseCameraMover.isSettedOriginalPos);
        if (currentPrefab is ISelectableMonster selectableMonster)
        {
            selectableMonster.expectedCls = cls;
            selectableMonster.Depetrification(cls, GetSetOriginalPos);
        }
    }
    void OnSelectedCardFromDeck(SelectableCard selectedCard)
    {
        var sameCard = selectedCard == currentSelectedCard;
        if(currentSelectedCard != null && !sameCard)
        {
            currentSelectedCard._isSelected = false;
            currentSelectedCard = selectedCard;
        }
        ScrollManager.Instance.currentSelectedCard = selectedCard;
    }
}
