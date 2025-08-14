using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
public class BattleButtonUI : MonoBehaviour
{

    class TweenProcess
    {
        public readonly float fadeOutValue = 1.0f;
        float fadeInValue = 0f;
        float fadeOutDuration = 1.0f;
        float fadeInDuration = 0.1f;
        public FadeSet fadeOutSet { get; private set;}
        public FadeSet fadeInSet { get; private set;}
        public Vector2TweenSetup moveSetUp { get; private set;}
        public Vector2TweenSetup moveSetToOriginal { get; private set;}
        public Vector2 moveEndValue { get; private set; }
        public TweenProcess(Vector2 originalPos)
        {
            fadeOutSet = new FadeSet(fadeOutValue,fadeOutDuration);
            fadeInSet = new FadeSet(fadeInValue,fadeInDuration);
            moveEndValue = originalPos + Vector2.up * 50f;
            moveSetUp = new Vector2TweenSetup(moveEndValue, fadeOutDuration);
            moveSetToOriginal = new Vector2TweenSetup(originalPos, fadeInDuration);
        }
    }

    Button battleButton;
    Image image;
    Text text;

    Vector2 originalPos;
    CancellationTokenSource buttonCls = new CancellationTokenSource();
    TweenProcess tweenProcess;
    Func<CancellationTokenSource> getCardCls;
    bool isFadingOut = false;
    public void Initialize(Func<bool> saveDeckData,Func<CancellationTokenSource> getCurrentCardCls)
    {
        battleButton = GetComponent<Button>();
        image = GetComponent<Image>();
        text = GetComponentInChildren<Text>();

        var transparent = 0f;
        GraphicAlphaChange(transparent);
        originalPos = image.rectTransform.anchoredPosition;
        tweenProcess = new TweenProcess(originalPos);
        battleButton.onClick.AddListener(() =>
        {
            buttonCls.Cancel();
            buttonCls.Dispose();
            buttonCls = new CancellationTokenSource();
            if(saveDeckData.Invoke())
            {
                return;
            }
        });
        getCardCls = getCurrentCardCls;
        FadeOutAndMove();
    }
    //スクロールが終わった時
    public async void FadeOutAndMove()
    {
        if (isFadingOut) return;
        isFadingOut = true;
        Debug.Log("現れます");
        var scrollCls = ScrollManager.Instance.scrollCls;
        var cardCls = getCardCls.Invoke();
        Debug.Log(cardCls);
        //ここのclsはカード押されたとき、scrollされたとき、fade中にbattleボタン押されたとき(buttonCls)
        var tripleCls = CancellationTokenSource.CreateLinkedTokenSource(scrollCls.Token,buttonCls.Token,cardCls.Token);
        var fadeOutSet = tweenProcess.fadeOutSet;
        var moveSet = tweenProcess.moveSetUp;
        var imageFadeOut = image.Fader(fadeOutSet).ToUniTask(tweenCancelBehaviour:TweenCancelBehaviour.KillAndCancelAwait,
            cancellationToken: tripleCls.Token);
        var textFadeOut = text.Fader(fadeOutSet).ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait, 
            cancellationToken: tripleCls.Token);
        var move = image.RectMover(moveSet).ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait, 
            cancellationToken:tripleCls.Token);

        try
        {
            await UniTask.WhenAll(imageFadeOut, textFadeOut,move);
        }
        catch (OperationCanceledException)
        {
            if(buttonCls.IsCancellationRequested)
            {
                var endValue = tweenProcess.fadeOutValue;
                image.rectTransform.anchoredPosition = tweenProcess.moveEndValue;
                GraphicAlphaChange(endValue);
            }
            else if(scrollCls.IsCancellationRequested || cardCls.IsCancellationRequested)
            {
                Debug.Log("スクロール、もしくはカードが押されたのでキャンセルされました");
                SetOriginal();
            }
        }
        finally { isFadingOut = false; }
    }
    void GraphicAlphaChange(float endValue)
    {
        image.AlphaChange(endValue);
        text.AlphaChange(endValue);
    }
    public void SetOriginal()
    {
        Debug.Log("最初のポジションに戻します");
        if (image.rectTransform.anchoredPosition == originalPos) return;
        var fadeInSet = tweenProcess.fadeInSet;
        var moveToOriginalSet = tweenProcess.moveSetToOriginal;
        var imageFadeIn = image.Fader(fadeInSet);
        var textFadeIn = text.Fader(fadeInSet);
        var moveToOriginal = image.RectMover(moveToOriginalSet);
    }
}
