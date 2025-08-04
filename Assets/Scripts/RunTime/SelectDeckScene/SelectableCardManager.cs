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
    public List<CardData> allCardDatas { get; set; } = new List<CardData>();
    [SerializeField] DeckChooseCameraMover deckChooseCameraMover;
    [SerializeField] Image parentImage;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Canvas parentCanvas;

    GameObject cardImagePrefab;
    List<SelectableCard> selectableCards = new List<SelectableCard>();
    SelectableCard currentSelectedCard;
    CancellationTokenSource cls = new CancellationTokenSource();
    Func<SelectableCard, PrefabBase> OnSelectedCard;
    List<UniTask> fadeTasks = new List<UniTask>();
    private void Awake()
    {
        if (Instance != null) Instance = null;
        Instance = this;
    }
    public async UniTask Initialize(Func<SelectableCard, PrefabBase> action, Action<int, int> lineSetAction)
    {
        OnSelectedCard = action;
        await  LineUpCards(lineSetAction);
    }
    async UniTask LineUpCards(Action<int, int> lineSetAction)
    {
        var cardDatas = (await SetFieldFromAssets.SetFieldByLabel<CardData>("CardData")).ToList();
        allCardDatas = cardDatas.OrderBy(data => data.SortOrder).ToList();
        cardImagePrefab = await SetFieldFromAssets.SetField<GameObject>("Prefabs/SelectableCard");
        if (cardImagePrefab == null) return;
        var criterioX = -530f;
        var criterioY = 530f;
        var spaceX = 145f;
        var spaceY = -250f;
        var dataCount = allCardDatas.Count;
        var columnCount = 5;
        var line = Mathf.CeilToInt((float)dataCount / columnCount);
        Debug.Log($"Line‚Í{line},ƒRƒ‰ƒ€‚Í{columnCount}");
        lineSetAction.Invoke(line, columnCount);
        var instanciatedCount = 0;
        var index = 0;
        for (int l = 0; l < line; l++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                var pos = new Vector2(criterioX + spaceX * c, criterioY + l * spaceY);
                var obj = Instantiate(cardImagePrefab);
                obj.transform.SetParent(parentImage.transform);
                var image = obj.GetComponent<Image>();
                if (image != null) image.rectTransform.localPosition = pos;
                var cmp = obj.GetComponent<SelectableCard>();
                if (cmp != null)
                {
                    cmp.cardData = allCardDatas[index];
                    selectableCards.Add(cmp);
                    UnityAction<bool> stopAction = (isDowned) => ScrollManager.Instance.isPointerDowned = isDowned;
                    cmp.Initialize(scrollRect, stopAction, OnSelectedCardChanged,parentCanvas,parentImage);
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
            if (card._isSelected) return;
            card.selectableCardImage.FadeOutIconImage(scrollCls);
        });
    }
    public void CardFadeIn()
    {
        selectableCards.ForEach(card =>
        {
            card.selectableCardImage.FadeInIconImage(cls);
        });
    }
    async void OnSelectedCardChanged(SelectableCard selectedCard)
    {
        var sameCard = currentSelectedCard == selectedCard;
        if (currentSelectedCard != null && !sameCard)
        {
            cls?.Cancel();
            cls?.Dispose();
           
            currentSelectedCard.selectableCardImage.SetOriginal();
            currentSelectedCard._isSelected = false;

            var previousPrefab = deckChooseCameraMover.currentSelectedPrefab;
            if (previousPrefab is ISelectableMonster monster)
            {
                monster.expectedCls = null;
                monster.Repetrification();
            }
            deckChooseCameraMover.currentSelectedPrefab = null;
        }
        cls = new CancellationTokenSource();
        var scrollCls = ScrollManager.Instance.cls;
        CardFadeOut(scrollCls);
        currentSelectedCard = selectedCard;
        ScrollManager.Instance.currentSelectedCard = selectedCard; 
        deckChooseCameraMover.selectedCardCls = cls;
        deckChooseCameraMover.currentSelectedPrefab = OnSelectedCard?.Invoke(selectedCard);
        await deckChooseCameraMover.MoveToFrontOfObj();
        var currentPrefab = deckChooseCameraMover.currentSelectedPrefab;
        Func<bool> GetSetOriginalPos = (() => deckChooseCameraMover.isSettedOriginalPos);
        if (currentPrefab is ISelectableMonster selectableMonster)//!sameCard &&
        {
            selectableMonster.expectedCls = cls;
            selectableMonster.Depetrification(cls,GetSetOriginalPos);
        }
    }
}
