using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class StatusUIAppear: MonoBehaviour
{
    [System.Serializable]
    class IconImages
    {
        [SerializeField] Sprite attackAmountSprite;
        [SerializeField] Sprite hpSprite;
        [SerializeField] Sprite chaseRangeSprite;
        [SerializeField] Sprite summonWaitTimeSprite;
        [SerializeField] Sprite attackRangeSprite;
        [SerializeField] Sprite perMoveStepSprite;
        [SerializeField] Sprite attackDistanceSprite;//近接か長距離（飛び道具で攻撃）
        [SerializeField] Sprite monsterTargetSprite;
        [SerializeField] Sprite monsterMoveSprite;
        [SerializeField] Sprite projectileMoveSpeedSprite;

        public Dictionary<IconType, Sprite> iconMap => new Dictionary<IconType, Sprite>
        {
            {IconType.attackAmount,attackAmountSprite},
            {IconType.Hp,hpSprite},
            {IconType.summonWaitTime,summonWaitTimeSprite},
            {IconType.attackRange,attackRangeSprite},
            {IconType.perMoveStep,perMoveStepSprite},
            {IconType.attackDistance,attackDistanceSprite},
            {IconType.monsterTarget,monsterTargetSprite},
            {IconType.monsterMove,monsterMoveSprite},
            {IconType.chaseRange,chaseRangeSprite},
            {IconType.projectileMoveSpeed,projectileMoveSpeedSprite},
        };

        public Dictionary<IconType, Text> textMap { get; set; } = new Dictionary<IconType, Text>();
        public Dictionary<IconType, Image> iconImageMap { get; set; } = new Dictionary<IconType, Image>();
        public Dictionary<IconType, Image> lusterImageMap { get; set; } = new Dictionary<IconType, Image>();
    }

    [SerializeField] IconImages iconImages;

    List<Text> texts = new List<Text>();
    List<Image> _iconImages = new List<Image>();
    List<Image> lusterBackGroundImages = new List<Image>();
    string lusterObjName = "Luster";

    List<Graphic> currentTargetGraphics = new List<Graphic>();
    CancellationTokenSource transparentCls = new CancellationTokenSource();
    List<UniTask> fadeTasks = new List<UniTask>();
    private void Start() => Setup();
    public async void ApperUI(MonsterStatusData monsterStatusData,CancellationTokenSource cls)
    {
        GetComponentsInChildren<Graphic>().ToList().ForEach(g =>
        {
            if (g.name == lusterObjName) return;
            g.gameObject.SetActive(false);
        });

        transparentCls?.Cancel();
        transparentCls?.Dispose();
        transparentCls = new CancellationTokenSource();
        if(fadeTasks.Count > 0)
        {
            fadeTasks.Clear();
            fadeTasks.TrimExcess();
        }
        //ここのclsはscrollClsとcard側のuseを押されたときのclsの二つ
        var statusDic = new Dictionary<IconType, string>
        {
            {IconType.attackAmount, monsterStatusData.AttackAmount.ToString()},
            {IconType.Hp,monsterStatusData.Hp.ToString()},
            {IconType.summonWaitTime,monsterStatusData.SummonWaitTime.ToString()},
            {IconType.attackRange,monsterStatusData.AttackRange.ToString()},
            {IconType.perMoveStep,(monsterStatusData.MoveSpeed * monsterStatusData.MoveStep).ToString()},
            {IconType.attackDistance,null},
            {IconType.monsterTarget,null},
            {IconType.monsterMove, monsterStatusData.MonsterMoveType.ToString()},
        };

        if(monsterStatusData.MonsterAttackType == MonsterAttackType.ToEveryThing)
        {
            statusDic.Add(IconType.chaseRange, monsterStatusData.ChaseRange.ToString());
        }
        if (monsterStatusData is ProjectileAttackMonsterStatus projectile)
        {
            statusDic.Add(IconType.projectileMoveSpeed, projectile.ProjectileMoveSpeed.ToString());
        }

        statusDic[IconType.attackDistance] = monsterStatusData.AttackType switch
        {
            AttackType.Simple => "Melee",
            AttackType.Long => "Ranged",
            _ => default,
        };

        statusDic[IconType.monsterTarget] = (monsterStatusData.MonsterAttackType,monsterStatusData.MonsterMoveType) switch
        {
            (MonsterAttackType.ToEveryThing,MonsterMoveType.Walk) => "Ground Only",
            (MonsterAttackType.ToEveryThing,MonsterMoveType.Fly) => "Ground & Air",
            (MonsterAttackType.OnlyBuilding,MonsterMoveType.Walk) or (MonsterAttackType.OnlyBuilding,MonsterMoveType.Fly) => "Building Only",
            _=> default,
        };

        var currentTargetLusterImages = new List<Image>();
        foreach (var keyValuePair in statusDic)
        {
            var type = keyValuePair.Key;
            if(iconImages.iconImageMap.TryGetValue(type,out var image)) image.gameObject.SetActive(true);
            if(iconImages.lusterImageMap.TryGetValue(type,out var lusterImage))
            {
                currentTargetLusterImages.Add(lusterImage);    
            }
            if (iconImages.textMap.TryGetValue(type, out var text))
            {
                text.gameObject.SetActive(true);
                var content = keyValuePair.Value;
                text.text = content;
            }
            else continue;
            currentTargetGraphics.Add(text);
            currentTargetGraphics.Add(image);
            currentTargetGraphics.Add(lusterImage);
        }

        var endAlpha = 1.0f;
        var duration = 0.5f;
        var fadeSet = new FadeSet(endAlpha,duration);

        statusDic.Clear();
        statusDic.TrimExcess();
        
        currentTargetGraphics.ForEach(g =>
        {
            var tween = g.Fader(fadeSet);
            var task = tween.ToUniTask(cancellationToken:cls.Token);
            fadeTasks.Add(task);
        });

        try
        {
            SlideLusterImage(currentTargetLusterImages,cls);
            await fadeTasks;
        }
        catch (OperationCanceledException) { CloseStatusUI(); } 
        fadeTasks.Clear();
        fadeTasks.TrimExcess();
    }
    void Setup()
    {
        var sortedImage = GetComponentsInChildren<Image>().ToList()
            .Where(image => image.gameObject.name != lusterObjName)
            .OrderBy(image => image.transform.GetSiblingIndex()).ToList();
        var sortedTexts = GetComponentsInChildren<Text>().ToList().OrderBy(text => text.transform.GetSiblingIndex()).ToList();
        texts = sortedTexts;
        _iconImages = sortedImage;

        var spriteDic = iconImages.iconMap;
        var imageDic = iconImages.iconImageMap;
        for (int i = 0; i < sortedImage.Count; i++)
        {
            var icon = sortedImage[i];
            TransparentGraphic(icon);
            IconType type = (IconType)i;
            imageDic[type] = icon;
            var sp = spriteDic[type];
            icon.sprite = sp;
        }

        var textDic = iconImages.textMap;
        var lusterDic = iconImages.lusterImageMap;
        for (int i = 0;i < texts.Count; i++)
        {
            var text = texts[i];
            var lusterImage = text.GetComponentInChildren<Image>();
            var type = (IconType)i;
            textDic[type] = text;
            lusterDic[type] = lusterImage;

            TransparentGraphic(text);          
        }
    }
    void TransparentGraphic(Graphic graphic)
    {
        var c = graphic.color;
        c.a = 0f;
        graphic.color = c;
    }
    public async void CloseStatusUI()
    {
        if(fadeTasks.Count > 0)
        {
            fadeTasks.Clear();
            fadeTasks.TrimExcess();
        }
        if (currentTargetGraphics == null) return;
        var endAlpha = 0f;
        var duration = 0.25f;
        var fadeSet = new FadeSet(endAlpha, duration);
        currentTargetGraphics.ForEach(g => fadeTasks.Add(g.Fader(fadeSet).ToUniTask(cancellationToken:transparentCls.Token)));

        try
        {
            await fadeTasks;
        }
        catch(OperationCanceledException)
        {
            return;
        }
        finally
        {
            currentTargetGraphics.Clear();
            currentTargetGraphics.TrimExcess();
        }
    }
    void SlideLusterImage(List<Image> currentLusterImages,CancellationTokenSource cls)
    {
        var direction = 1;
        var moveSpeed = 200f;
        var absAmount = 400f;
        var delay = 1.5f;
        Func<Image,Vector2,UniTask> imageMover = async (lusterImage,originalPos) =>
        {
            try
            {
                var originalPosX = originalPos.x;
                var currentPos = lusterImage.rectTransform.anchoredPosition;
                var targetPos = new Vector2(originalPosX + absAmount * direction, currentPos.y);
                Debug.Log(targetPos);
                while (!cls.IsCancellationRequested)
                {
                    currentPos = lusterImage.rectTransform.anchoredPosition;
                    var newPos = Vector2.MoveTowards(currentPos, targetPos,Time.deltaTime * moveSpeed);
                    lusterImage.rectTransform.anchoredPosition = newPos;

                    if (Mathf.Approximately(newPos.x,targetPos.x))
                    {
                        direction = -direction;
                        targetPos = new Vector2(originalPosX + absAmount * direction, currentPos.y);
                        Debug.Log(targetPos);
                        await UniTask.Delay(TimeSpan.FromSeconds(delay),cancellationToken:cls.Token);
                    }

                    await UniTask.Yield(cancellationToken: cls.Token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                lusterImage.rectTransform.anchoredPosition = originalPos;
            }
        };

        for (var i = 0;i < currentLusterImages.Count;i++) 
        {
            var lusterImage = currentLusterImages[i];
            var originalPos = lusterImage.rectTransform.anchoredPosition;
            imageMover(lusterImage,originalPos);
        }
    }
}
public enum IconType
{
    attackAmount,
    Hp,
    summonWaitTime,
    attackRange,
    perMoveStep,
    attackDistance,//近接か長距離（飛び道具で攻撃）
    monsterTarget,//建物（今のところはタワー）だけかユニットと建物の両方か
    monsterMove,
    chaseRange,
    projectileMoveSpeed
}


