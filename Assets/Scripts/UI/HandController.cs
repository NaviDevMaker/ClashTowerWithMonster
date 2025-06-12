using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System;
public class HandController
{
    public Queue<Card> myDeck { get;private set; }//��D�ɂ���J�[�h

    public UnityAction<List<Card>,Card> SetFirstHandOnUI;
    public UnityAction<Card> NextCardSetOnUI;
    public UnityAction<Card,int> OnUsedCard;
    public UnityAction<Card,List<Card>> OnChangedNextCard;

    int deckCount = 8;
    int handCount = 4;

    List<Card> currentHand = new List<Card>();
    Card nextCard;
    public HandController(List<Card> myDeck,UnityAction<List<Card>, Card> action,UnityAction<Card> action1
        ,UnityAction<Card,int> action2,UnityAction<Card,List<Card>> action3)
    {
        SetFirstHandOnUI = action;
        NextCardSetOnUI = action1;
        OnUsedCard = action2;
        OnChangedNextCard = action3;

        this.myDeck = new Queue<Card>(myDeck);
        if(this.myDeck != null && myDeck.Count == deckCount)
        {
            Initialize();
        }
    }

    void Initialize()
    {
        Debug.Log("�ŏ��̎�D���擾���܂�");
        var firstHand = new List<Card>();
        var queueToList = new List<Card>(myDeck);
        queueToList.Shuffle();
        myDeck = new Queue<Card>(queueToList);
        for(int i = 0; i < handCount;i++)
        {
            firstHand.Add(myDeck.Dequeue());
        }

        currentHand = firstHand;
        firstHand.ForEach(hand => Debug.Log(hand.CardData.CardName));//�m�F�p
        nextCard = myDeck.Dequeue();
        SetFirstHandOnUI?.Invoke(firstHand,nextCard);
        NextCardProcess(true);


        foreach (var deckCard in myDeck)
        {
            deckCard.gameObject.SetActive(false);
        }
    }

    public void NextCardToMyHand(Card usedCard)
    {
        Debug.Log("�g���܂���");
        var index = currentHand.IndexOf(usedCard);
        currentHand[index] = null;
    
        myDeck.Enqueue(usedCard);
        usedCard.gameObject.SetActive(false);
        //nextcard�ɐݒ肳��Ă����J�[�h����D�ɓ����
        NextCardProcess(false);
        OnUsedCard?.Invoke(nextCard,index);

        currentHand[index] = nextCard;
        currentHand[index].gameObject.SetActive(true);
        nextCard = myDeck.Dequeue();
        //����nextcard��nextcard�̈ʒu�ɓ����
        NextCardProcess(true);
    }

    void NextCardProcess(bool isSetted)
    {
        nextCard.isSettedNextCard = isSetted;
        if (isSetted)
        {
            NextCardSetOnUI?.Invoke(nextCard);
            OnChangedNextCard?.Invoke(nextCard,currentHand);//Handmanager��nextcard��deck�ɂ����̕ϐ�����
        }

    }
}
