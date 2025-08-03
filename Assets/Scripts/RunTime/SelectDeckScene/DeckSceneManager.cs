using System;
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        Action<int,int> action = selectablePrefabManager.SetLine;
        await cardManager.Initialize(GetSelectedCardPrefab, action);
        selectablePrefabManager.Initialize();
        UnityAction<BaseEventData> positionSetEvent = deckChooseCameraMover.SetOriginalPos;
        UnityAction fadeInEvent = cardManager.CardFadeIn;
        scrollManager.Initialize(positionSetEvent,fadeInEvent);
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
}
