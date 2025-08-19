using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Mono.Cecil;
using System;
using DG.Tweening;
using static UnityEngine.UI.Image;

public class PrefabBase : MonoBehaviour
{
    public int sortOrder;//Ç±ÇÍÇÃêîílÇcardDataÇÃãKíËÇÃÇ‚Ç¬Ç…çáÇÌÇπÇÈ
    public CardType cardType { get; set;}
    public float offsetZ { get; protected set; }
    GameObject selectedDeckParticle;
    public enum PrefabScale
    { 
       small,
       middle,
       large,
       spell,
    }

    [SerializeField] PrefabScale prefabScale;
    protected class TweenInfo
    {
        public  Vector3 scaleEndValue;
        readonly Vector3 scaleOriginal = Vector3.zero;

        public readonly float duration = 0.5f;
        public readonly float originalDuration = 0.1f;
        public Vector3TweenSetup scaleSet; 
        public Vector3TweenSetup scaleToZero;
        public TweenInfo()
        {
            scaleToZero = new Vector3TweenSetup(scaleOriginal, originalDuration);
        }
    }

    TweenInfo tweenInfo;
    public virtual void Initialize() { tweenInfo = new TweenInfo();}
    public async void SetSelectedEffect(CancellationTokenSource removedCls)
    {
        var magicCircleEffect = EffectManager.Instance.magicCircleEffect;
        if (selectedDeckParticle == null)
        {
            selectedDeckParticle = Instantiate(magicCircleEffect.summonPointerParticle);
            var originalPos = transform.position;
            var pos = PositionGetter.GetFlatPos(originalPos);
            selectedDeckParticle.transform.position = pos;
            selectedDeckParticle.gameObject.SetActive(false);
            var newColor = cardType switch
            {
                CardType.Monster => (Color?)null,
                CardType.Spell => ColorUtility.TryParseHtmlString("#6A5ACD", out var c) ? (Color?)c : null,
                _ => (Color?)null,
            };

            var originalScale = selectedDeckParticle.transform.localScale;

            var scale = prefabScale switch
            {
                PrefabScale.small => originalScale,
                PrefabScale.middle or PrefabScale.spell => originalScale * 2.0f,
                PrefabScale.large => originalScale * 3.0f,
                _=> default
            };

            var scaleEndValue = tweenInfo.scaleEndValue = scale;
            var duration = tweenInfo.duration;
            tweenInfo.scaleSet = new Vector3TweenSetup(scaleEndValue, duration);
            if (newColor != null)
            {
                Debug.Log("êFïœÇ¶Ç‹Ç∑");
                var main = selectedDeckParticle.GetComponent<ParticleSystem>().main;
                main.startColor = (Color)newColor;
            }

        }

        try
        {
            selectedDeckParticle.gameObject.SetActive(true);      

            var duration = tweenInfo.duration;
            var scaleSet = tweenInfo.scaleSet;        
            var scaleTween = selectedDeckParticle.Scaler(scaleSet);
            var rotateTask = GetRotateTask(selectedDeckParticle,duration,removedCls);
            var task = scaleTween.ToUniTask(tweenCancelBehaviour: TweenCancelBehaviour.KillAndCancelAwait,
                cancellationToken: removedCls.Token);
            await task;
        }
        catch (OperationCanceledException)
        {
            ScalerToZero();
            return;
        }
        magicCircleEffect.PointerSummonEffect(selectedDeckParticle, removedCls.Token).Forget();
    }
    public async void ScalerToZero()
    {
        var originalDuration = tweenInfo.originalDuration;
        var originalScaleSet = tweenInfo.scaleToZero;
        var originalScaleTask = selectedDeckParticle.Scaler(originalScaleSet).ToUniTask();
        var rotateToOriginal = GetRotateTask(selectedDeckParticle,originalDuration,null);
        await UniTask.WhenAll(originalScaleTask, rotateToOriginal);
        selectedDeckParticle.gameObject.SetActive(false);
    }
    async UniTask GetRotateTask(GameObject origin,float duration, CancellationTokenSource cls)
    {
        var token = cls?.Token ?? CancellationToken.None;
        var startEuler = origin.transform.rotation.eulerAngles;
        var startY = startEuler.y;
        var time = 0f;

        while (time < duration)
        {
            if (cls != null && cls.IsCancellationRequested)
                break;

            time += Time.deltaTime;
            var lerp = Mathf.Clamp01(time / duration);
            var eulerY = Mathf.Lerp(startY, startY + 360f, lerp);

            origin.transform.rotation = Quaternion.Euler(startEuler.x, eulerY, startEuler.z);

            await UniTask.Yield(cancellationToken: token);
        }

        origin.transform.rotation = Quaternion.Euler(startEuler.x, startY + 360f, startEuler.z);
    }
}
