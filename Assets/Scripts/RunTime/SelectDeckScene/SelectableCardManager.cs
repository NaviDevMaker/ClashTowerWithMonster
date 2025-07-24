using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class SelectableCardManager : MonoBehaviour
{

    List<CardData> allCardDatas = new List<CardData>();
    [SerializeField] Image parentImage;
    [SerializeField] ScrollRect scrollRect;
    GameObject cardImagePrefab;
    List<SelectableCard> selectableCards = new List<SelectableCard>();
    SelectableCard currentSelectedCard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void Initialize()
    {
        LineUpCards();
    }
    async void LineUpCards()
    {
        allCardDatas = (await SetFieldFromAssets.SetFieldByLabel<CardData>("CardData")).ToList();
        cardImagePrefab = await SetFieldFromAssets.SetField<GameObject>("Prefabs/SelectableCard");
        if (cardImagePrefab == null) return;
        var criterioX = -530f;
        var criterioY = 530f;
        var spaceX = 145f;
        var spaceY = -250f;
        var dataCount = allCardDatas.Count;
        var columnCount = 5f;
        var line = dataCount / columnCount;

        var instanciatedCount = 0;
        for (int l = 0; l < line; l++)
        {
            for (int c = 0; c < columnCount; c++)
            {
                var pos = new Vector2(criterioX + spaceX * c, criterioY + l * spaceY);
                var obj = Instantiate(cardImagePrefab);
                obj.transform.SetParent(parentImage.transform);
                var image = obj.GetComponent<Image>();
                if (image != null) image.rectTransform.localPosition = pos;
                var cmp = obj.GetComponent<SelectableCard>();
                if (cmp != null)
                {
                    var index = l * c;
                    cmp.cardData = allCardDatas[index];
                    selectableCards.Add(cmp);
                    UnityAction<bool> stopAction = (isDowned) => ScrollManager.Instance.isPointerDowned = isDowned;
                    cmp.Initialize(scrollRect, stopAction,OnSelectedCardChanged);
                    instanciatedCount++;
                    if (instanciatedCount == dataCount) break;
                }
            }
        }
    }
    void OnSelectedCardChanged(SelectableCard selectedCard)
    {
        if(currentSelectedCard != null)
        {
            currentSelectedCard._isSelected = false;
        }
        currentSelectedCard = selectedCard;
    }
}
