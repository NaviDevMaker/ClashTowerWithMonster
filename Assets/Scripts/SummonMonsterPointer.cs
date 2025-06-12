
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
public class SummonMonsterPointer : MonoBehaviour
{
    GameObject summonPointerParticle;
    GameObject selectedCardPrefab;
    Dictionary<CardName,GameObject> cardPrefabs = new Dictionary<CardName, GameObject>();
    Card currentCard;

    public UnityAction OnPointerUp;
    public Func<bool> CheckCanSetMonster;
    public UnityAction<Card> OnSummonMonster;
    bool onTheField = false;

    Vector3 particlePos = Vector3.zero;
    CancellationTokenSource cts = new CancellationTokenSource();
    
    private void Update()
    {
        if ((InputManager.IsClickedSummonButton() && CheckCanSetMonster.Invoke()) 
            || InputManager.IsClickedSummonButtonOnHandField())
        {
            if(selectedCardPrefab != null && onTheField)
            {
                SetMonsterOnField();
            }           
        }
    }
    void LateUpdate()
    {
        SumonPointDisplay();
        PrefabActiveChange();
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
        var hits = Physics.RaycastAll(ray);
        if(hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var hitLayer = 1 << hit.collider.gameObject.layer;
                if (Layers.groundLayer == hitLayer)
                {
                    onTheField = true;
                    Debug.Log("ヒット");
                    var targetPos = hit.point;
                    if (selectedCardPrefab != null)
                    {
                        if (summonPointerParticle != null)
                        {
                            if (!summonPointerParticle.activeSelf) summonPointerParticle.gameObject.SetActive(true);
                        }
                        selectedCardPrefab.gameObject.transform.position = targetPos;
                        particlePos = targetPos;
                        targetPos.y += 0.5f;
                        summonPointerParticle.transform.position = targetPos;
                    }
                    break;
                }
            }        
        }
        else
        {
            onTheField = false;
            EnactivePointerEffect();
        }
    }
    void SetMonsterOnField()
    {
        summonPointerParticle.gameObject.SetActive(false);
        cts.Cancel();
        cts.Dispose();
        cts = new CancellationTokenSource();
        var obj = Instantiate(selectedCardPrefab, selectedCardPrefab.transform.position, Quaternion.identity);

        var summonbable = obj.GetComponent<ISummonbable>();
        summonbable.isSummoned = true;
        StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(particlePos,currentCard.CardData.CardType));
        OnSummonMonster?.Invoke(currentCard);
        OnPointerUp?.Invoke();
    }
    
    //選ばれているカードが変更されたとき
    public void SetMonsterPrefab(Card selectedCard)
    {
       
        if (selectedCardPrefab != null)
        {
          var currentCardData = currentCard.CardData;
          //前回のプレファブを非表示
          if (cardPrefabs.TryGetValue(currentCardData.CardName, out GameObject previousPrefab))
          {              
               previousPrefab.gameObject.SetActive(false);
          }
        }
        if(cardPrefabs.TryGetValue(selectedCard.CardData.CardName,out GameObject cardPrefab))
        {
              cardPrefab.gameObject.SetActive(true);
              SetSummonPointerEffect();
              selectedCardPrefab = cardPrefab;
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
        if (selectedCardPrefab != null)
        {
            var currentCardData = currentCard.CardData;
            if (cardPrefabs.TryGetValue(currentCardData.CardName, out GameObject currentPrefab))
            {
                currentPrefab.gameObject.SetActive(false);
                selectedCardPrefab = null;
            }
        }
        currentCard = null;
    }

    public void EnactivePointerEffect()
    {
        if(summonPointerParticle != null) summonPointerParticle.SetActive(false);
    }

    void PrefabActiveChange()
    {
        if (selectedCardPrefab != null)
        {
            var currentCardData = currentCard.CardData;
            if (cardPrefabs.TryGetValue(currentCardData.CardName, out GameObject currentPrefab))
            {
                currentPrefab.gameObject.SetActive(onTheField);
            }
        }
    }

}
