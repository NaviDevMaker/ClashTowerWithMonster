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
        [System.Serializable]
        public class SpecificSpellIconImage
        {
            public Sprite confusionSprite;
            public Sprite freezeSprite;
            public Sprite shapeShiftSprite;
        }

        [SerializeField] SpecificSpellIconImage specificSpellIconImage;
        [SerializeField] Sprite attackAmountSprite;
        [SerializeField] Sprite hpSprite;
        [SerializeField] Sprite chaseRangeSprite;
        [SerializeField] Sprite summonWaitTimeSprite;
        [SerializeField] Sprite attackRangeSprite;
        [SerializeField] Sprite perMoveStepSprite;
        [SerializeField] Sprite attackDistanceSprite;//近接か長距離（飛び道具で攻撃）
        [SerializeField] Sprite TargetSprite;
        [SerializeField] Sprite targetCountSprite;
        [SerializeField] Sprite monsterMoveSprite;
        [SerializeField] Sprite projectileMoveSpeedSprite;
        [SerializeField] Sprite continuousTimeSpellSprite;
        [SerializeField] Sprite castTimeSprite;
        [SerializeField] Sprite spellDamageAmountSprite;
        [SerializeField] Sprite spellHealAmountSprite;
        [SerializeField] Sprite spellDurationSprite;

        public Dictionary<IconType, Sprite> iconMap => new Dictionary<IconType, Sprite>
        {
            {IconType.attackAmount,attackAmountSprite},
            {IconType.Hp,hpSprite},
            {IconType.summonWaitTime,summonWaitTimeSprite},
            {IconType.attackRange,attackRangeSprite},
            {IconType.perMoveStep,perMoveStepSprite},
            {IconType.attackDistance,attackDistanceSprite},
            {IconType.Target,TargetSprite},
            {IconType.targetCount,targetCountSprite},
            {IconType.monsterMove,monsterMoveSprite},
            {IconType.chaseRange,chaseRangeSprite},
            {IconType.projectileMoveSpeed,projectileMoveSpeedSprite},
            {IconType.freeze,specificSpellIconImage.freezeSprite},
            {IconType.confusion,specificSpellIconImage.confusionSprite},
            {IconType.shapeShift,specificSpellIconImage.shapeShiftSprite},
            {IconType.continuousTimeSpell,continuousTimeSpellSprite},
            {IconType.castTimeSpell,castTimeSprite},
            {IconType.spellDamageAmount,spellDamageAmountSprite},
            {IconType.spellHealAmount,spellHealAmountSprite},
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
    public async void ApperUI(ScriptableObject statusData,CancellationTokenSource cls)
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
        var statusDic =  statusData is MonsterStatusData monsterStatusData ? GetMonsterStatusContent(monsterStatusData)
            : statusData is SpellStatus spellStatus ? GetSpellStatusContent(spellStatus) : null;
        if (statusDic == null) return;

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
        Debug.Log(iconImages.iconMap.Count);
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
      
        var moveSpeed = 400f;
        var absIncreaseAmount = 600f;
        var delay = 0.75f;
        Func<Image,Vector2,UniTask> imageMover = async (lusterImage,originalPos) =>
        {
            var direction = 1;
            var absAmount = 1000f;
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
                        absAmount += absIncreaseAmount * direction;
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

    Dictionary<IconType,string> GetMonsterStatusContent(MonsterStatusData monsterStatusData)
    {
        var statusDic = new Dictionary<IconType, string>
        {
            {IconType.attackAmount, monsterStatusData.AttackAmount.ToString()},
            {IconType.Hp,monsterStatusData.Hp.ToString()},
            {IconType.summonWaitTime,monsterStatusData.SummonWaitTime.ToString()},
            {IconType.attackRange,monsterStatusData.AttackRange.ToString()},
            {IconType.perMoveStep,(monsterStatusData.MoveSpeed * monsterStatusData.MoveStep).ToString()},
            {IconType.attackDistance,null},
            {IconType.Target,null},
            {IconType.monsterMove, monsterStatusData.MonsterMoveType.ToString()},
        };

        if (monsterStatusData.MonsterAttackType == MonsterAttackType.RelyOnMoveType)
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

        statusDic[IconType.Target] = (monsterStatusData.MonsterAttackType, monsterStatusData.MonsterMoveType) switch
        {
            (MonsterAttackType.RelyOnMoveType, MonsterMoveType.Walk) => "Ground Only",
            (MonsterAttackType.RelyOnMoveType, MonsterMoveType.Fly) => "Ground & Air",
            (MonsterAttackType.ToEveryThing,MonsterMoveType.Walk)
              or (MonsterAttackType.ToEveryThing, MonsterMoveType.Fly) => "Everything",
            (MonsterAttackType.OnlyBuilding, MonsterMoveType.Walk) 
            or (MonsterAttackType.OnlyBuilding, MonsterMoveType.Fly) => "Building Only",
            _ => default,
        };

        return statusDic;
    }

    Dictionary<IconType, string> GetSpellStatusContent(SpellStatus spellStatus)
    {
        var statusDic = new Dictionary<IconType, string>();

        var specificSpellIconType = spellStatus.SpecificSpellType switch
        {
            SpecificSpellType.Freeze => IconType.freeze,
            SpecificSpellType.Confusion => IconType.confusion,
            SpecificSpellType.ShapeShift => IconType.shapeShift,
            _=> default,
        };
        
        var spellIconType = spellStatus.SpellType switch
        {
            SpellType.Damage or SpellType.DamageToEveryThing => IconType.spellDamageAmount,
            SpellType.Heal => IconType.spellHealAmount,
            _ => default
        };

        var targetContent = spellStatus.SpellType switch
        {
            SpellType.Damage or SpellType.OtherToEnemyside => "Enemy",
            SpellType.DamageToEveryThing or SpellType.OtherToEverything => "EveryThing",
            SpellType.Heal or SpellType.OtherToPlayerside => "Ally",
            _ => default

        };

        statusDic[IconType.Target] = targetContent;
        var countContent = spellStatus.TargetCount.ToString();
        statusDic[IconType.targetCount] = $"TargetCount :{countContent}";

        var spellInvokeIconType = spellStatus.InvokeType switch
        {
            SpellInvokeType.Continuous => IconType.continuousTimeSpell,
            SpellInvokeType.CastTime => IconType.castTimeSpell,
            _ => default
        };

        var targetSpeficType = SpecificSpellType.ShapeShift | SpecificSpellType.Freeze | SpecificSpellType.Confusion;
        var mySpecificType = spellStatus.SpecificSpellType;
        if((spellStatus.SpecificSpellType & targetSpeficType) != 0)
        {
            var content = $"{mySpecificType.ToString()}";
            statusDic[specificSpellIconType] = content;
        }

        var targetSpellType = SpellInvokeType.CastTime | SpellInvokeType.Continuous;
        var myInvokeType = spellStatus.InvokeType;
        if((myInvokeType & targetSpellType) != 0)
        {
            var duration = spellStatus.SpellDuration.ToString();
            var type = myInvokeType.ToString();
            var content = $"{type}:{duration}";
            statusDic[spellInvokeIconType] = content;
        }
        
        var effectiveTarget = SpellType.Damage | SpellType.DamageToEveryThing | SpellType.Heal;
        if ((spellStatus.SpellType & effectiveTarget) != 0 )
        {
            var amount = spellStatus.EffectAmont;
            if (amount != 0)
            {
                var amountContent = amount.ToString();
                statusDic[spellIconType] = amountContent;
            }
        }

        return statusDic;
    }
}
public enum IconType
{
    attackAmount,//攻撃力
    Hp,
    summonWaitTime,//召喚されるまでの時間
    attackRange,
    perMoveStep,//一歩ごとに進む距離
    attackDistance,//近接か長距離（飛び道具で攻撃）
    Target,//建物（今のところはタワー）だけかユニットと建物の両方か
    targetCount,//スペルのターゲットについて、範囲内の該当ユニット全部か指定されたユニットの数か
    monsterMove,//地面を進むか空を飛んでいるか
    chaseRange,//追跡範囲、この範囲内に敵がいなかったらタワーに進む
    projectileMoveSpeed,//飛び道具で攻撃するユニットの飛び道具の速度
    continuousTimeSpell,
    castTimeSpell,
    spellDamageAmount,
    spellHealAmount,
    confusion,
    freeze,
    shapeShift,

}


