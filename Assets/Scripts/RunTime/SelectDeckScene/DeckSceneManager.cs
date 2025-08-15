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
        Action<int,int> action = selectablePrefabManager.SetLine;
        Action<MonsterStatusData, CancellationTokenSource> apppearStatusAction = statusUIAppear.ApperUI;
        UnityAction battleButtonToOriginal = battleButtonUI.SetOriginal;
        UnityAction positionSetEvent = deckChooseCameraMover.SetOriginalPos;
        UnityAction closeStatusUIEvent = statusUIAppear.CloseStatusUI;

        await cardManager.Initialize(GetSelectedCardPrefab, action,apppearStatusAction, 
            GetMonsterStatusDataAndPrefab,battleButtonToOriginal,positionSetEvent,closeStatusUIEvent);
        selectablePrefabManager.Initialize(GetMonsterStatusDataAndPrefab);
        UnityAction fadeInEvent = cardManager.CardFadeIn;
        UnityAction fadeOutBattleButtonEvent = battleButtonUI.FadeOutAndMove;
        scrollManager.Initialize(positionSetEvent,fadeInEvent,closeStatusUIEvent
            ,fadeOutBattleButtonEvent,battleButtonToOriginal);
        Func<CancellationTokenSource> getCurrentCls = cardManager.GetClickedCancellationTokenSource;
        Func<bool> saveDeck = cardManager.GetChoosenDeckDatas;
        battleButtonUI.Initialize(saveDeck,getCurrentCls);
    }
    PrefabBase GetSelectedCardPrefab(SelectableCard selectableCard)
    {
        var targetIndex = selectableCard.cardData.SortOrder;
        var prefabs = selectablePrefabManager.monsters;
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
}
