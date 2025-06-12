using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using System.Linq;

public class HandManager : MonoBehaviour
{
    [SerializeField] List<Card> myHands;

    Card nextCard;
    List<Card> currentHand;
    
    [SerializeField] SummonMonsterPointer sumonMonsterPointer;
    [SerializeField] MyHandField handFieid;
    [SerializeField] EnergyGageController energyGageController;
    bool visible = true;

    Card selectedCard = null;
    Card previousCard = null;

    private void Awake()
    {
        handFieid.OnStartGame = GetDeck;
    }
    private void Start()
    {
        Set();
        sumonMonsterPointer.GetMonsterPrefab(myHands);
        sumonMonsterPointer.OnPointerUp = ResetCardHands;
    }
    void Update()
    {
        if(InputManager.IsClickedSwitchHandDisplay())
        {
            PlayerhandsDisplay();
        }

        if(InputManager.IsClickedResetSelectedCard())
        {
            ResetCardHands();
            sumonMonsterPointer.EnactivePointerEffect();
        }
    }
    void Set()
    {
        foreach(Card card in myHands)
        {
            card.OnSelectedCard = SetSelectedCard;
        }

        Func<bool> check = () => handFieid.canSumonMonster;
        sumonMonsterPointer.CheckCanSetMonster = check;
        handFieid.Initialize(ref sumonMonsterPointer.OnSummonMonster,SetNextCardAndDeckCard);
        sumonMonsterPointer.OnSummonMonster += energyGageController.ReduceEnergy;
    }
    void PlayerhandsDisplay()
    {
        visible = !visible;

        currentHand.ForEach(hand => hand.gameObject.SetActive(visible));

        nextCard.gameObject.SetActive(visible);
        handFieid.gameObject.SetActive(visible);
    }
    void SetSelectedCard(Card currentCard)
    {
        if(selectedCard != null)
        {
            previousCard = selectedCard;//前回のカードを取得
            previousCard.currentSelected = false;
            previousCard._cardImage.SetOriginal(previousCard,isHandCard:!previousCard.isSettedNextCard);
        }    
        Debug.Log("カードセット");
        selectedCard = currentCard;
        Card.CardSelected = true;
        sumonMonsterPointer.SetMonsterPrefab(currentCard);
     
        foreach (var hand in myHands)
        {
            if (selectedCard == hand) continue;
            hand._cardImage.SetNewAlpha();
        }
    }

    void ResetCardHands()
    {
        if (selectedCard != null)
        {
            selectedCard.currentSelected = false;
            selectedCard = null;
        }

        previousCard = null;
        Card.CardSelected = false;
        sumonMonsterPointer.UnactiveCurrentPrefab();
        myHands.ForEach(hand => {
            hand._cardImage.SetOriginal(hand,isHandCard:!hand.isSettedNextCard);
        });
    }

    List<Card> GetDeck()
    {
       if(myHands != null)  return myHands;
        return default;
    }

    void SetNextCardAndDeckCard(Card nextCard_handCont,List<Card> currentHand)
    {
        this.nextCard = nextCard_handCont;
        this.currentHand = currentHand;
    }


}
