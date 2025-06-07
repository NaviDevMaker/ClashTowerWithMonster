
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem.DualShock.LowLevel;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Threading;
public class SumonMonsterPointer : MonoBehaviour
{
    GameObject summonPointerParticle;
    GameObject selectedMonster;
    Dictionary<CardName,GameObject> cardPrefabs = new Dictionary<CardName, GameObject>();
    Card currentCard;

    public UnityAction OnPointerUp;
    public Func<bool> CheckCanSetMonster;
    public UnityAction<Card> OnSummonMonster;

    Vector3 particlePos = Vector3.zero;
    CancellationTokenSource cts = new CancellationTokenSource();
    
    private void Update()
    {
        if ((InputManager.IsClickedSummonButton() && CheckCanSetMonster.Invoke()) 
            || InputManager.IsClickedSummonButtonOnHandField())
        {
            if(selectedMonster != null)
            {
                SetMonsterOnField();
            }           
        }
    }
    void LateUpdate()
    {
        SumonPointDisplay();
    }

    public void GetMonsterPrefab(List<Card> hands)
    {
        foreach (Card card in hands)
        {
            var prefab = Instantiate(card.CardData.CardPrefab);
            cardPrefabs[card.CardData.CardName] = prefab;
            prefab.gameObject.SetActive(false);
        }
    }
    void SumonPointDisplay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var hitLayer = 1 << hit.collider.gameObject.layer;
            if (Layers.groundLayer == hitLayer)
            {
                Debug.Log("ƒqƒbƒg");
                var targetPos = hit.point;
                if (selectedMonster != null)
                {
                    selectedMonster.gameObject.transform.position = targetPos;
                    particlePos = targetPos;
                    targetPos.y += 0.5f;
                    summonPointerParticle.transform.position = targetPos;
                }
            }
        } 
    }

    void SetMonsterOnField()
    {
        summonPointerParticle.gameObject.SetActive(false);
        cts.Cancel();
        cts.Dispose();
        cts = new CancellationTokenSource();
        Instantiate(selectedMonster, selectedMonster.transform.position, Quaternion.identity);
       
        StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(particlePos));
        OnSummonMonster?.Invoke(currentCard);
        OnPointerUp?.Invoke();
    }
    
    public void SetMonsterPrefab(Card selectedCard)
    {
       if(selectedMonster != null)
       {
          var currentCardData = currentCard.CardData;
          if (cardPrefabs.TryGetValue(currentCardData.CardName, out GameObject previousPrefab))
          {              
               previousPrefab.gameObject.SetActive(false);
          }
       }
       if(cardPrefabs.TryGetValue(selectedCard.CardData.CardName,out GameObject cardPrefab))
       {
          cardPrefab.gameObject.SetActive(true);
          SetSummonPointerEffect();
          selectedMonster = cardPrefab;
          currentCard = selectedCard;
       }
    }

    void SetSummonPointerEffect()
    {
        if (summonPointerParticle == null)
        {
            summonPointerParticle = Instantiate(EffectManager.Instance.magicCircleEffect.summonPointerParticle);
        }       
        EffectManager.Instance.magicCircleEffect.PointerSummonEffect(summonPointerParticle,cts.Token).Forget();
        summonPointerParticle.gameObject.SetActive(true);
    }
    public void UnactiveCurrentPrefab()
    {
        if (selectedMonster != null)
        {
            var currentCardData = currentCard.CardData;
            if (cardPrefabs.TryGetValue(currentCardData.CardName, out GameObject currentPrefab))
            {
                currentPrefab.gameObject.SetActive(false);
                selectedMonster = null;
            }
        }
        currentCard = null;

    }
}
