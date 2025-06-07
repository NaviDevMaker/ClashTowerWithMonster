using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

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
        handController = new HandController(OnStartGame?.Invoke(), SetCardUIOnMyHand, SetNextCardUI, SetHandToCardPos,action3);
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
        card._cardImage.SetOriginal(card,isHandCard:!card.isSettedNextCard);
        UIFuctions.ShakeText(card._cardImage.iconImage);
    }
    void SetNextCardUI(Card nextCard)
    {
        var sceleAmount = 0.75f;
        var pos = new Vector2(-766, -134);
        var scele = Vector2.one * sceleAmount;
        nextCard._cardImage.iconImage.rectTransform.anchoredPosition = pos;  
        nextCard._cardImage.iconImage.transform.localScale = scele;
        nextCard.gameObject.SetActive(true);
    }
}
