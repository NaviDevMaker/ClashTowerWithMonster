using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MyHandField : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    HandController handController;
    public Func<List<Card>> OnStartGame;

    Vector2 cardOffset = Vector2.zero;
    float cardOffsetY = -150.0f;
    public bool canSumonMonster { get; private set; } = false;

    public void Initialize(ref UnityAction<Card> action,UnityAction<Card,List<Card>> action3)
    {
        cardOffset = new Vector2(-400, cardOffsetY);
        handController = new HandController(OnStartGame?.Invoke(), SetCardUIOnMyHand, SetNextCardUI, SetHandToCardPos,action3,SetFirstCardOnMyHandPos);
        action = handController.NextCardToMyHand;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        canSumonMonster = false;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        canSumonMonster = true;
    }
    private void OnDisable()
    {
        canSumonMonster = true;
    }

    void SetCardUIOnMyHand(List<Card> firstHand,Card nextCard)
    {
        for (int i = 0; i < firstHand.Count; i++)
        {
            SetHandToCardPos(firstHand[i], i);
        }
    }
    void SetHandToCardPos(Card card,int cardIndex)
    {
        var width = 270f;
        var anchoredPos = cardOffset + new Vector2(width * cardIndex, 0f);
        card._cardImage.iconImage.rectTransform.anchoredPosition = anchoredPos;
        card._cardImage.SetOriginal(card, isHandCard: !card.isSettedNextCard);
        card._cardImage.iconImage.ShakeUI();
    }

    async UniTask SetFirstCardOnMyHandPos(List<Card> firstHand)
    {
        for (int i = 0; i < firstHand.Count;i++)
        {
            var card = firstHand[i];
            SetNextCardUI(card);
            card._cardImage.iconImage.rectTransform.rotation = Quaternion.Euler(0f, 0f, -45f);
            var width = 270f;
            var anchoredPos = cardOffset + new Vector2(width * i, 0f);
            var moveAndScaleDuration = 0.2f;
            var rotationDuration = 0.1f;
            var targetScale = card._cardImage.originalScale;
            var sequence = DOTween.Sequence();
            var task = sequence.Append(card._cardImage.iconImage.rectTransform.DOAnchorPos(anchoredPos, moveAndScaleDuration).SetEase(Ease.Linear))
               .Join(card._cardImage.iconImage.rectTransform.DOScale(targetScale, moveAndScaleDuration).SetEase(Ease.Linear))
               .Join(card._cardImage.iconImage.rectTransform.DORotate(Vector3.zero, rotationDuration)).ToUniTask();
            await task;
        }      
    }
    void SetNextCardUI(Card nextCard)
    {
        var scaleAmount = 0.75f;
        var pos = new Vector2(-766, -134);
        var scele = Vector2.one * scaleAmount;
        nextCard._cardImage.iconImage.rectTransform.anchoredPosition = pos;  
        nextCard._cardImage.iconImage.transform.localScale = scele;
        nextCard.gameObject.SetActive(true);
    }
}
