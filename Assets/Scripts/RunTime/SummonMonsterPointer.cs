
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Game.Spells;
using System.Linq;
using Unity.VisualScripting;
using Game.Monsters;
public class SummonMonsterPointer : MonoBehaviour
{
    [SerializeField] NeedEnergyDisplayer needEnergyDisplayer;
    GameObject summonPointerParticle;
    GameObject selectedCardPrefab;
    Dictionary<CardName,GameObject> cardPrefabs = new Dictionary<CardName, GameObject>();
    //最初に場所を示すプレファブのほうのISummonbableを取得する
    Dictionary<CardName, SpellBase> summonbables = new Dictionary<CardName,SpellBase>();
    // " UnitBaseを取得する
    Dictionary<CardName,IMonster> monsters = new Dictionary<CardName,IMonster>();
    Card currentCard;

    LineRenderer lineRenderer;
    public UnityAction OnPointerUp;
    public Func<bool> CheckCanSetMonster;
    public UnityAction<Card> OnSummonMonster;// Func<Card,bool>
    public Func<int> GetCurrentEnergy_SummonPointer;
    public UnityAction<Card> OnSuccessSummon;
    bool onTheField = false;

    Vector3 particlePos = Vector3.zero;
    CancellationTokenSource cts = new CancellationTokenSource();
    ISummonbable summonbable;
    private void Start()
    {
        SetUpLineRenderer();
        needEnergyDisplayer.SetText();
        Func<float,float,Material,UniTask> waitAction = async (radAdjust,maxIntencity,material) =>
        {
            var baseColor = material.GetColor("_EmissionColor");
            while (!this.IsDestroyed())
            {
                var amount = (Mathf.Cos(Time.time * radAdjust) * 0.5f + 0.5f) * maxIntencity;
                material.SetColor("_EmissionColor", baseColor * amount);
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        };
        lineRenderer.LitLineRendererMaterial(waitAction);
        Debug.Log(lineRenderer);
    }
    private void Update()
    {
        if (currentCard == null) return;
        var cardEnergy = currentCard.CardData.Energy;
        var currentEnergy = (int)GetCurrentEnergy_SummonPointer?.Invoke();
        if ((InputManager.IsClickedSummonButton() && CheckCanSetMonster.Invoke())
            || InputManager.IsClickedSummonButtonOnHandField())
        {
            if (selectedCardPrefab != null && onTheField && currentEnergy >= cardEnergy) SetCardOnField();
        }

        if (currentEnergy <= cardEnergy)
        {
            if (needEnergyDisplayer != null)
            {
                if (!needEnergyDisplayer.gameObject.activeSelf) needEnergyDisplayer.gameObject.SetActive(true);
            }
            needEnergyDisplayer.RenewEnergyText(currentEnergy, cardEnergy);
        }
        else EnactiveNeedEnergy();
    }
    void LateUpdate()
    {
        SummonPointDisplay();
        PrefabActiveChange();
    }

    void SetUpLineRenderer()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        lineRenderer.SetUpLineRenderer();
    }

    public void GetMonsterPrefab(List<Card> hands)
    {
        foreach (Card card in hands)
        {
            var prefab = Instantiate(card.CardData.CardPrefab);
            cardPrefabs[card.CardData.CardName] = prefab;
            if(card.CardData.CardType == CardType.Spell)
            { summonbables[card.CardData.CardName] = prefab.GetComponent<SpellBase>();}
            else if(card.CardData.CardType == CardType.Monster)
            { monsters[card.CardData.CardName] = prefab.GetComponent<IMonster>();}
            prefab.gameObject.SetActive(false);
        }
    }
    void SummonPointDisplay()
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
                    var unitTargetPos = hit.point;
                    if (selectedCardPrefab != null)
                    {
                        if (summonPointerParticle != null)
                        {
                            if (!summonPointerParticle.activeSelf) summonPointerParticle.gameObject.SetActive(true);
                        }
                        targetPos.y += 0.5f;
                        var cardType = currentCard.CardData.CardType;
                        unitTargetPos.y += cardType == CardType.Monster ? 0.5f :0f;
                        if(monsters.TryGetValue(currentCard.CardData.CardName,out var monster))
                        {
                            if(monster._UnitType == UnitType.monster && monster._FlyingMonsterStatus != null)
                            {
                                var flyingOffsetY = monster._FlyingMonsterStatus.FlyingOffsetY;
                                unitTargetPos.y += flyingOffsetY;
                            }
                        }
                        selectedCardPrefab.gameObject.transform.position = unitTargetPos;
                        particlePos = targetPos;
                        //targetPos.y += 0.5f;//場合によっては直して
                        summonPointerParticle.transform.position = targetPos;
                        var displayerPos = targetPos + needEnergyDisplayer.transform.right * 3.0f;
                        needEnergyDisplayer.gameObject.transform.position = displayerPos;
                        if (currentCard.CardData.CardType == CardType.Spell)
                        {
                            var radiusX = 0f;
                            var radiusZ = 0f;
                            if (summonbables.TryGetValue(currentCard.CardData.CardName, out var spellBase))
                            {
                                radiusX = spellBase.rangeX;
                                radiusZ = spellBase.rangeZ;
                                var offsetY = 0.5f;
                                lineRenderer.DrawRange(targetPos, radiusX, radiusZ,offsetY);
                            }
                        }
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
            EnactiveNeedEnergy();
        }
    }
    void SetCardOnField()
    {
        OnSummonMonster.Invoke(currentCard);
        EnactivePointerEffect();
        EnableLineRenderer();

        cts.Cancel();
        cts.Dispose();
        cts = new CancellationTokenSource();
        var pos = selectedCardPrefab.transform.position;
        var rot = selectedCardPrefab.transform.rotation;
        var obj = Instantiate(selectedCardPrefab,pos,rot);
        if(obj.TryGetComponent<ISpawner>(out var multiSpawnMonster))
        {
            obj.gameObject.SetActive(false);
            multiSpawnMonster.SpawnMonsters(pos,rot,AlphaChange);
        }
        else SetStartCondition(obj);

        SetSummonedCardName(obj);
        AppearSummonedUI(obj);
        StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(particlePos,currentCard.CardData.CardType));
        OnSuccessSummon?.Invoke(currentCard);
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
               if (currentCardData.CardType == CardType.Monster)
               {
                   var unit = monsters[currentCard.CardData.CardName];
               }
               previousPrefab.gameObject.SetActive(false);
          }
        }
        var selectedCardData = selectedCard.CardData;
        if(cardPrefabs.TryGetValue(selectedCardData.CardName,out GameObject cardPrefab))
        {
              if (selectedCardData.CardType == CardType.Monster)
              {
                    var monster = monsters[selectedCardData.CardName];
                    if(monster is ISpawner multi)
                    {
                        var count = multi._SpawnCount;
                        for (int i = 0; i < count; i++)
                        {
                           var childMonster = multi.parent.GetChild(i).gameObject;
                           AlphaChange(childMonster.GetComponent<IMonster>());
                        }
                    }
                    else AlphaChange(monster);
              }
              cardPrefab.gameObject.SetActive(true);
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

    public void EnactiveNeedEnergy()
    {
        if(needEnergyDisplayer != null) needEnergyDisplayer.gameObject.SetActive(false);
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
               currentPrefab.gameObject.SetActive(onTheField); 
            }
        }
    }

    void AlphaChange(IMonster monster,bool isSummoned = false)
    {
        if (monster == null) return;
        var mesh = monster._BodyMesh;
        var material = mesh.material;
        var color = material.color;
        if (isSummoned) color.a = 1.0f;
        else { var translusent = 0.5f; color.a = translusent; }       
        material.color = color;
    }
    void SetStartCondition(GameObject obj)
    {
        var summondCardData = currentCard.CardData;
        if (summondCardData.CardType == CardType.Monster)
        {
           var monster = obj.GetComponent<IMonster>();
           if (monster != null) AlphaChange(monster, true);
        }
    }
    void SetSummonedCardName(GameObject obj)
    {
        var summonedCardData = currentCard.CardData;
        summonbable = obj.GetComponent<ISummonbable>();
        if (summonbable != null)
        {
            var name = summonedCardData.CardName.ToString();
            summonbable.SummonedCardName = name;
            summonbable.isSummoned = true;
        }
    }
    void AppearSummonedUI(GameObject obj)
    {
        var monoCmp = obj.GetComponent<MonoBehaviour>();
        var type = monoCmp.GetType();
        var method = typeof(UIManager).GetMethod("SummonedNameDisplay")
                .MakeGenericMethod(type);
        method.Invoke(UIManager.Instance, new object[] { monoCmp });
    }
}
