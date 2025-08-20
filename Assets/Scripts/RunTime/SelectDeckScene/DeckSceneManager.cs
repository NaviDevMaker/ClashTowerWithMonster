using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DeckSceneManager : MonoBehaviour
{
    [SerializeField] SelectableCardManager cardManager;
    [SerializeField] SelectablePrefabManager selectablePrefabManager;
    [SerializeField] DeckChooseCameraMover deckChooseCameraMover;
    [SerializeField] ScrollManager scrollManager;
    [SerializeField] StatusUIAppear statusUIAppear;
    [SerializeField] BattleButtonUI battleButtonUI;


   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        Action<int,int> setLineAction = selectablePrefabManager.SetLine;
        Action<ScriptableObject, CancellationTokenSource> apppearStatusAction = statusUIAppear.ApperUI;
        UnityAction battleButtonToOriginal = battleButtonUI.SetOriginal;
        UnityAction positionSetEvent = deckChooseCameraMover.SetOriginalPos;
        UnityAction closeStatusUIEvent = statusUIAppear.CloseStatusUI;
        UnityAction<PrefabBase> unableLineRenderer = selectablePrefabManager.UnableLineRenderer;
        UnityAction enableLineRenderer = selectablePrefabManager.EnableLineRenderer;
        UnityAction fadeInEvent = cardManager.CardFadeIn;
        UnityAction fadeOutBattleButtonEvent = battleButtonUI.FadeOutAndMove;
        Func<CancellationTokenSource> getCurrentCls = cardManager.GetClickedCancellationTokenSource;
        Func<bool> saveDeck = cardManager.GetChoosenDeckDatas;

        var cardManagerActions = new CardManagerActions
        {
            onSelectedCardAction = GetSelectedCardPrefab,
            lineSetAction = setLineAction,
            apperStatusUIAction = apppearStatusAction,
            getStatusAndPrefabAction = GetMonsterStatusDataAndPrefab,
            getSpellStatusAndPrefabAction = GetSpellStatusDataAndPrefab,
            setBattleButtonToOriginal = battleButtonToOriginal,
            cameraPositionSetAction = positionSetEvent,
            closeStatusUIAction = closeStatusUIEvent,
            unableLineRenderer = unableLineRenderer,
            enableLineRenderer = enableLineRenderer
        };

        var prefabManagerActions = new PrefabManagerActions
        { 
            getMosnterDataAndPrefab = GetMonsterStatusDataAndPrefab,
            getSpellDataAndPrefab = GetSpellStatusDataAndPrefab
        };


        var scrollManagerActions = new ScrollManagerActions
        { 
            setCameraPosToOriginal = positionSetEvent,
            enableLineRenderer = enableLineRenderer,
            fadeInAction = fadeInEvent,
            closeStatusUIAction = closeStatusUIEvent,
            fadeOutBattleButton = fadeOutBattleButtonEvent,
            transparentBattleButton = battleButtonToOriginal
        };

        var battleButtonUIActions = new BattleButtonUIActions
        { 
            saveDeckData = saveDeck,
            getCurrentCardCls = getCurrentCls,
        };

        await cardManager.Initialize(cardManagerActions);/*GetSelectedCardPrefab, setLineAction,apppearStatusAction, 
            GetMonsterStatusDataAndPrefab,battleButtonToOriginal,positionSetEvent,closeStatusUIEvent*/
        selectablePrefabManager.Initialize(prefabManagerActions);/*GetMonsterStatusDataAndPrefab,GetSpellStatusDataAndPrefab*/
       
        scrollManager.Initialize(scrollManagerActions);/*positionSetEvent,fadeInEvent,closeStatusUIEvent
          ,fadeOutBattleButtonEvent,battleButtonToOriginal*/

        battleButtonUI.Initialize(battleButtonUIActions);/*saveDeck,getCurrentCls*/
    }
    PrefabBase GetSelectedCardPrefab(SelectableCard selectableCard)
    {
        var targetIndex = selectableCard.cardData.SortOrder;
        var prefabs = selectablePrefabManager.prefabs;
        for (int i = 0; i < prefabs.Count; i++)
        {
            if(i == targetIndex)
            {
                return prefabs[i];
            }
        }
        return null;
    }
    public (MonsterStatusData,SelectableMonster) GetMonsterStatusDataAndPrefab(SelectableCard selectableCard)
    {
        var targetIndex = selectableCard.cardData.SortOrder;
      
        for (int i = 0; i < selectablePrefabManager.monsters.Count; i++)
        {
            if (i == targetIndex)
            {
                var monster = selectablePrefabManager.monsters[i];
                var data = monster._statusData;
                return (data,monster);
            }
        }
        return (null,null);
    }

    public (SpellStatus,SelectableSpell) GetSpellStatusDataAndPrefab(SelectableCard selectableCard)
    {
        var targetIndex = selectableCard.cardData.SortOrder;
        var startIndex = selectablePrefabManager.monsters.Count;
        var spellIndex = targetIndex - startIndex;
        if(spellIndex >= 0 && spellIndex < selectablePrefabManager.spells.Count)
        {
            var spell = selectablePrefabManager.spells[spellIndex];
            var data = spell._spellStatus;
            return (data, spell);
        }
        return (null, null);
    }
}

public class CardManagerActions
{
    public Func<SelectableCard, PrefabBase> onSelectedCardAction;
    public Action<int, int> lineSetAction;
    public Action<ScriptableObject, CancellationTokenSource> apperStatusUIAction;
    public Func<SelectableCard, (MonsterStatusData, SelectableMonster)> getStatusAndPrefabAction;
    public Func<SelectableCard, (SpellStatus, SelectableSpell)> getSpellStatusAndPrefabAction;
    public UnityAction setBattleButtonToOriginal;
    public UnityAction cameraPositionSetAction;
    public UnityAction closeStatusUIAction;
    public UnityAction<PrefabBase> unableLineRenderer;
    public UnityAction enableLineRenderer;
}

public class PrefabManagerActions
{
   public Func<SelectableCard, (MonsterStatusData, SelectableMonster prefab)> getMosnterDataAndPrefab;
   public Func<SelectableCard, (SpellStatus, SelectableSpell prefab)> getSpellDataAndPrefab;
}
public class ScrollManagerActions
{
    public UnityAction setCameraPosToOriginal;
    public UnityAction enableLineRenderer;
    public UnityAction fadeInAction;
    public UnityAction closeStatusUIAction;
    public UnityAction fadeOutBattleButton;
    public UnityAction transparentBattleButton;
}
public class BattleButtonUIActions
{
    public Func<bool> saveDeckData;
    public Func<CancellationTokenSource> getCurrentCardCls;
}
