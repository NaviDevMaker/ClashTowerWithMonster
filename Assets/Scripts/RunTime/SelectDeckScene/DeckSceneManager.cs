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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        Action<int,int> action = selectablePrefabManager.SetLine;
        Action<MonsterStatusData, CancellationTokenSource> apppearStatusAction = statusUIAppear.ApperUI;
        await cardManager.Initialize(GetSelectedCardPrefab, action,apppearStatusAction, GetMonsterStatusDataAndPrefab);
        selectablePrefabManager.Initialize();
        UnityAction<BaseEventData> positionSetEvent = deckChooseCameraMover.SetOriginalPos;
        UnityAction fadeInEvent = cardManager.CardFadeIn;
        UnityAction closeStatusUIEvent = statusUIAppear.CloseStatusUI;
        scrollManager.Initialize(positionSetEvent,fadeInEvent,closeStatusUIEvent);
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
