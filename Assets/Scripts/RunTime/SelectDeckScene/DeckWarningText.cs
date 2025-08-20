
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DeckWarningText : MonoBehaviour
{
    class TweenInfo
    {
        public readonly float opaqueDuration = 0.5f;
        float transparentDuration = 3.0f;
        float moveAmount = 125f;
        float opaqueEndValue = 1.0f;
        public readonly float transparentEndValue = 0f;
        public Vector2 moveTextEndValue { get; private set; }
        public Vector2 moveImageEndValue { get; private set; }
        public FadeSet opaqueSet { get; private set;}
        public FadeSet transparentSet { get; private set;}
        public Vector2TweenSetup moveTextSet { get; private set;}
        public Vector2TweenSetup moveImageSet { get; private set; }
        public Vector2TweenSetup moveTextToOriginalSet { get; private set; }
        public Vector2TweenSetup moveImageToOriginalSet { get; private set; }

        public TweenInfo(Vector2 originalTextPos,Vector2 originalImagePos)
        {
            moveTextEndValue = new Vector2(originalTextPos.x,originalTextPos.y + moveAmount);
            moveImageEndValue = new Vector2(originalImagePos.x,originalImagePos.y + moveAmount);
            opaqueSet = new FadeSet(opaqueEndValue,opaqueDuration);
            transparentSet = new FadeSet(transparentEndValue, transparentDuration);
            moveTextSet = new Vector2TweenSetup(moveTextEndValue, opaqueDuration);
            moveImageSet = new Vector2TweenSetup(moveImageEndValue,opaqueDuration);
            moveTextToOriginalSet = new Vector2TweenSetup(originalTextPos,transparentDuration);
            moveImageToOriginalSet = new Vector2TweenSetup(originalImagePos, transparentDuration);
        }
    }
    class ComponentInfo
    {
        public Text warningText { get; set;}
        public Image wariningImage { get; set; }
        public Vector2 originalTextPos { get; set; }
        public Vector2 originalImagePos { get; set; }
    }

    
    TweenInfo tweenInfo;
    ComponentInfo componentInfo;
    CancellationTokenSource appearCls = new CancellationTokenSource();
    SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
    void Start()
    {
        Setup();
    }
    public async void AppearWarningText(CancellationTokenSource cardCls)
    {
        Debug.Log($"{cardCls.IsCancellationRequested},{appearCls.IsCancellationRequested}");
        appearCls?.Cancel();
        appearCls?.Dispose();
        appearCls = new CancellationTokenSource();
        //Debug.Log("警告文を表示します");

        var doubleCls = CancellationTokenSource.CreateLinkedTokenSource(cardCls.Token,appearCls.Token);
        var opaqueSet = tweenInfo.opaqueSet;
        var moveTextSet = tweenInfo.moveTextSet;
        var moveImageSet = tweenInfo.moveImageSet;
        var transparent = tweenInfo.transparentEndValue;

        try
        {
            await semaphoreSlim.WaitAsync();
            var warningText = componentInfo.warningText;
            var warningImage = componentInfo.wariningImage;
            warningText.gameObject.SetActive(true);
            warningImage.gameObject.SetActive(true);
            //カードが選ばれたときのcls
            
            var opaqueTweens = new Tween[]
            {
                warningText.Fader(opaqueSet),
                warningText.RectMover(moveTextSet),
                warningImage.Fader(opaqueSet),
                warningImage.RectMover(moveImageSet)
            };
            var opaqueTask = AlphaSequenceTaskGetter(opaqueTweens, doubleCls);
            await opaqueTask;
            var waitDuration = 2.5f;
            await UniTask.Delay(TimeSpan.FromSeconds(waitDuration), cancellationToken: doubleCls.Token);
            var transparentSet = tweenInfo.transparentSet;
            var moveTextToOriginalSet = tweenInfo.moveTextToOriginalSet;
            var moveImageToOriginalSet = tweenInfo.moveImageToOriginalSet;

            var transparentTweens = new Tween[]
            {
                warningText.Fader(transparentSet),
                warningText.RectMover(moveTextToOriginalSet),
                warningImage.Fader(transparentSet),
                warningImage.RectMover(moveImageToOriginalSet)
            };

            var transparentTask = AlphaSequenceTaskGetter(transparentTweens, doubleCls);
            await transparentTask;
        }      
        catch (OperationCanceledException)
        {
            Debug.Log("キャンセルされました");
        }
        finally
        { 
            SetToOriginal(transparent);
            semaphoreSlim.Release();
        }      
    }

    void SetToOriginal(float endValue)
    {
        var warningText = componentInfo.warningText;
        var warningImage = componentInfo.wariningImage;
        var originalTextPos = componentInfo.originalTextPos;
        var originalImagePos = componentInfo.originalImagePos;
        Debug.Log("元の通りに戻します");
        warningText.AlphaChange(endValue);
        warningImage.AlphaChange(endValue);
        warningText.rectTransform.anchoredPosition = originalTextPos;
        warningImage.rectTransform.anchoredPosition = originalImagePos;
        warningText.gameObject.SetActive(false);
        warningImage.gameObject.SetActive(false);
    }
    UniTask AlphaSequenceTaskGetter(Tween[] tweens,CancellationTokenSource cls)
    {
        var sequence = DOTween.Sequence();
        for (int i = 0; i < tweens.Length; i++)
        {
            var tween = tweens[i];
            sequence.Join(tween);
        }
        var task = sequence.ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait,
            cancellationToken: cls.Token);
        return task;
    }

    void Setup()
    {
        var warningText = GetComponent<Text>();
        var textSiblingIndex = warningText.transform.GetSiblingIndex();
        var warningImage = warningText.transform.parent.GetChild(textSiblingIndex + 1).GetComponent<Image>();
        var originalTextPos = warningText.rectTransform.anchoredPosition;
        var originalTextColor = warningText.color;
        var originalImagePos = warningImage.rectTransform.anchoredPosition;
        var originalImageColor = warningImage.color;
        var transparent = 0f;
        warningText.AlphaChange(transparent);
        warningImage.AlphaChange(transparent);

        tweenInfo = new TweenInfo(originalTextPos,originalImagePos);
        componentInfo = new ComponentInfo
        { 
            originalImagePos = originalImagePos,
            originalTextPos = originalTextPos,
            warningText = warningText,
            wariningImage = warningImage
        };
    }
}
