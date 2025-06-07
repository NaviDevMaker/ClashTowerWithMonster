using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using System.Threading;
using DG.Tweening;


public class CanMoveTimerSetter:MonoBehaviour
{
    public static CanMoveTimerSetter Instance { get; private set; }

    GameObject timer;

    struct TimerImages
    {
        public Image outSideImage;
        public Image inSideImage;
        public Image secondHandImage;
    }

    private async void Awake()
    {
        Instance = this;
        timer = await SetFieldFromAssets.SetField<GameObject>("UI/Timer");
    }

    public async UniTask StartTimer(float summonTime,UnitBase targetUnit)
    {
        if(timer == null) timer = await SetFieldFromAssets.SetField<GameObject>("UI/Timer");

        var scale = targetUnit.UnitScale;
        var size = Vector3.zero;
        switch (scale)
        {
            case UnitScale.small:
                size = Vector3.one * 0.007f;
                break;
            case UnitScale.middle:
            case UnitScale.large:
                break;
        }

        var pos = targetUnit.transform.position + Vector3.up;
        var timerObj = Instantiate(this.timer, pos, Quaternion.identity);
        timerObj.transform.localScale = size;
        var parent = timerObj.transform.GetChild(0);
        var timerImages = new TimerImages
        {
            outSideImage = parent.GetChild(0).GetComponent<Image>(),
            inSideImage = parent.GetChild(0).GetChild(0).gameObject.transform.GetComponent<Image>(),
            secondHandImage = parent.GetChild(0).GetChild(1).gameObject.transform.GetComponent<Image>(),
        };
        
        timerImages.outSideImage.fillAmount = 0f;
        timerImages.inSideImage.fillAmount = 0f;
        var time = 0f;
        var originalRot = timerImages.secondHandImage.transform.localRotation;
        if(timerObj != null)
        {
            while (time < summonTime)
            {
                UIFuctions.LookToCamera(timerObj);
                time += Time.deltaTime;
                var ratio = time / summonTime;
                var rotateAmount = 360f * ratio;
                timerImages.outSideImage.fillAmount = ratio;
                timerImages.inSideImage.fillAmount = ratio;
                timerImages.secondHandImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -rotateAmount) * originalRot;
                await UniTask.Yield();
            }
        }

        timerImages.outSideImage.fillAmount = 1.0f;
        timerImages.inSideImage.fillAmount = 1.0f;

        var originalScale = timerObj.transform.localScale;
        var endvalue_increase = originalScale * 3.0f;
        var endValue_down = originalScale * 0.5f;
        var duration = 0.15f;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(timerObj.transform.DOScale(endvalue_increase, duration))
            .Append(timerObj.transform.DOScale(endValue_down, duration))
            .Join(timerImages.inSideImage.DOFade(0f, duration))
            .Join(timerImages.outSideImage.DOFade(0f, duration));
        await sequence.AsyncWaitForCompletion();
        UnityEngine.Object.Destroy(timerObj);
    }
}
