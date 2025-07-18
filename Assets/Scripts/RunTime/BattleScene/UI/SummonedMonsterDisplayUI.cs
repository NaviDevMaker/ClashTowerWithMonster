using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class SummonedMonsterDisplayUI : MonoBehaviour
{
    public class SummmonedCardLogInfo
    {
        GameObject imagePrefab;
        List<Sprite> changedImage;
        public Vector3 originalPos_me { get; set; } = Vector3.zero;
        public Vector3 originalScale { get; set; } = Vector3.zero;
        public float originalWidth { get; set; } = 0f;
        public float usualTargetX { get; set; } = 0f;
        public GameObject ImagePrefab => imagePrefab;
        public List<Sprite> ChangedImage => changedImage;

        public SummmonedCardLogInfo(GameObject imagePrefab,List<Sprite> changedImage)
        {
            this.imagePrefab = imagePrefab;
            this.changedImage = changedImage;
            imagePrefab.gameObject.SetActive(false);
            originalPos_me = imagePrefab.transform.localPosition;
            originalScale = imagePrefab.transform.localScale;
            var imageCmp = imagePrefab.transform.GetChild(0).GetComponent<Image>();
            originalWidth = imageCmp.rectTransform.rect.width;
            usualTargetX = -930f;
        }
    }

    [SerializeField] GameObject imagePrefab;
    [SerializeField] List<Sprite> changedImage;
    SummmonedCardLogInfo summmonedCardLogInfo;
    Queue<GameObject> activeSummonedUI = new Queue<GameObject>();
    private void Start() => summmonedCardLogInfo = new SummmonedCardLogInfo(imagePrefab,changedImage);
    public void SummonedUIDisplay<T>(T summonedObj) where T : MonoBehaviour,ISide
    {
        if (summonedObj.TryGetComponent<ISummonbable>(out var summonbable))
        {
            var name = summonbable.SummonedCardName;
            var isMine = summonedObj.GetUnitSide(0) == Side.PlayerSide;
            Debug.Log(isMine);
            RPG_ShowSummonLog(name, isMine);
        }       
    }
    async void RPG_ShowSummonLog(string unitName, bool isMine)
    {
        var prefab = summmonedCardLogInfo.ImagePrefab;
        var parent = prefab.transform.GetComponentInParent<Canvas>().gameObject.transform;
        var obj = PoolObjectPreserver.SummonedUIObjGetter();
        if (obj == null)
        {
            obj = Instantiate(prefab);
            if(!obj.activeSelf) obj.SetActive(true);
            PoolObjectPreserver.SummonedUIImagesObj.Add(obj);
        }
        activeSummonedUI.Enqueue(obj);
        var image = obj.transform.GetChild(0).GetComponent<Image>();
        var yOffset = (activeSummonedUI.Count - 1) * 50f;
        var adjustedPos =  summmonedCardLogInfo.originalPos_me + Vector3.down * yOffset;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = adjustedPos;
        obj.transform.localScale = summmonedCardLogInfo.originalScale;

        var index = isMine ? 0 : 1;
        image.sprite = summmonedCardLogInfo.ChangedImage[index];
        var content = $"{unitName}Ç™è¢ä´Ç≥ÇÍÇ‹ÇµÇΩ";
        var logText = obj.transform.GetChild(1).GetComponent<Text>();
        if(logText != null) logText.text = content;
        var maxLetterLength = 12;
        var letterLength = content.Length;
        var widthAmount = 16f;
        var isLong = letterLength > maxLetterLength ? true : false;
        int extra = 0;
        var newWidth = WidthGetter(letterLength, maxLetterLength,widthAmount,isLong,out extra);
        var usualTargetX = summmonedCardLogInfo.usualTargetX;
        var targetWidth = isLong ? usualTargetX - widthAmount * extra : usualTargetX + widthAmount * extra;   
        var duration = 0.5f;
        var delay = 2.0f;

        var tweens = new List<Tween>();
        tweens.Add(UIFuctions.SlideUI(image, duration, targetWidth, delay));
        tweens.Add(UIFuctions.SlideUI(logText, duration, targetWidth, delay));
        var tasks = TaskGetter(tweens);
        await UniTask.WhenAll(tasks);
        activeSummonedUI.Dequeue();
        obj.gameObject.SetActive(false);
    }

    List<UniTask> TaskGetter(List<Tween> tweens)
    { 
        var tasks = new List<UniTask>();
        tweens.ForEach(tween =>
        {
            var task = tween.ToUniTask(TweenCancelBehaviour.Complete);
            tasks.Add(task);
        });
        return tasks;
    }
    float WidthGetter(int letterLength,int maxLetterLength,float amount,bool isLong,out int extra)
    {
        if (letterLength == maxLetterLength)
        {
            extra = 0;
            return summmonedCardLogInfo.originalWidth;
        }
        var currentLength = maxLetterLength;
        var newWidth = summmonedCardLogInfo.originalWidth;
        var inclimentCount = 0;
        while (currentLength != letterLength)
        {
            var incliment = 1;
            if (!isLong)
            {
                amount = -amount;
                incliment = -incliment;
            }
            newWidth = summmonedCardLogInfo.originalWidth + amount;
            currentLength += incliment;
            inclimentCount++;
        }
        extra = inclimentCount;
        return newWidth;     
    }
}   
 

