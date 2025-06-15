
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Game.Spells;
using System.Linq;
public class SummonMonsterPointer : MonoBehaviour
{
    GameObject summonPointerParticle;
    GameObject selectedCardPrefab;
    Dictionary<CardName,GameObject> cardPrefabs = new Dictionary<CardName, GameObject>();
    //最初に場所を示すプレファブのほうのISummonbableを取得する
    Dictionary<CardName, SpellBase> summonbables = new Dictionary<CardName,SpellBase>();
    Card currentCard;

    LineRenderer lineRenderer;
    public UnityAction OnPointerUp;
    public Func<bool> CheckCanSetMonster;
    public UnityAction<Card> OnSummonMonster;
    bool onTheField = false;

    Vector3 particlePos = Vector3.zero;
    CancellationTokenSource cts = new CancellationTokenSource();
    ISummonbable summonbable;
    private void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        Debug.Log(lineRenderer);
    }
    private void Update()
    {
        if ((InputManager.IsClickedSummonButton() && CheckCanSetMonster.Invoke()) 
            || InputManager.IsClickedSummonButtonOnHandField())
        {
            if(selectedCardPrefab != null && onTheField)
            {
                SetCardOnField();
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
            if(card.CardData.CardType == CardType.Spell)
            {
                summonbables[card.CardData.CardName] = prefab.GetComponent<SpellBase>();
            }
            prefab.gameObject.SetActive(false);
        }
    }
    void SumonPointDisplay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray);
        if(hits.Length > 0)
        {
            //Spellのレイヤーだけの時はそれはgroundにhitしてないことを意味する
            if(hits.Length == 1)
            {
                var hit = hits.FirstOrDefault().collider.gameObject;
                var hitLayer = 1 << hit.gameObject.layer;
                var hitOnlySpell = hitLayer == Layers.spellLayer;
                if (hitOnlySpell)
                {
                    onTheField = false;
                    EnactivePointerEffect();
                    EnableLineRenderer();
                    return;
                }
            }
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
                        targetPos.y += 0.5f;
                        selectedCardPrefab.gameObject.transform.position = targetPos;
                        particlePos = targetPos;
                        //targetPos.y += 0.5f;//場合によっては直して
                        summonPointerParticle.transform.position = targetPos;

                        if (currentCard.CardData.CardType == CardType.Spell) DrawSpellRange(targetPos);
                        else EnableLineRenderer();
                    }
                    break;
                }
            }        
        }
        else
        {          
            onTheField = false;
            EnactivePointerEffect();
            EnableLineRenderer();
        }
    }
    void SetCardOnField()
    {
        EnactivePointerEffect();
        EnableLineRenderer();

        cts.Cancel();
        cts.Dispose();
        cts = new CancellationTokenSource();
        var obj = Instantiate(selectedCardPrefab, selectedCardPrefab.transform.position,selectedCardPrefab.transform.rotation);

        summonbable = obj.GetComponent<ISummonbable>();
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
        var selectedCardData = selectedCard.CardData;
        if(cardPrefabs.TryGetValue(selectedCardData.CardName,out GameObject cardPrefab))
        {           
               cardPrefab.gameObject.SetActive(true);//if(selectedCardData.CardType == CardType.Monster)
              //else if(selectedCardData.CardType == CardType.Spell) cardPrefab.gameObject.SetActive(false);
              SetSummonPointerEffect();
              selectedCardPrefab = cardPrefab;
              currentCard = selectedCard;
              if (selectedCardData.CardType == CardType.Spell) cardPrefab.gameObject.SetActive(false);
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

    public void EnableLineRenderer()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    void PrefabActiveChange()
    {
        if (selectedCardPrefab != null)
        {
            var currentCardData = currentCard.CardData;
            if (cardPrefabs.TryGetValue(currentCardData.CardName, out GameObject currentPrefab))
            {
               currentPrefab.gameObject.SetActive(onTheField);//if(currentCardData.CardType == CardType.Monster) 
            }
        }
    }

    void DrawSpellRange(Vector3 center)
    {
        if(!lineRenderer.enabled) lineRenderer.enabled = true;
        var radiusX = 0f;
        var radiusZ = 0f;
        if(summonbables.TryGetValue(currentCard.CardData.CardName,out var spellBase))
        {
            radiusX = spellBase.rangeX;
            radiusZ = spellBase.rangeZ;
        }
        var segument = 100;
        lineRenderer.positionCount = segument + 1;
        lineRenderer.loop = true;
        for (int i = 0; i <= segument; i++)
        {
            var angle = ((float)i / segument) * Mathf.PI * 2;
            var x = Mathf.Cos(angle) * radiusX;
            var z = Mathf.Sin(angle) * radiusZ;
            var nextPos = new Vector3(x, 0, z) + center;
            nextPos.y = Terrain.activeTerrain.SampleHeight(nextPos);
            lineRenderer.SetPosition(i, nextPos);
        }
    }
}
