using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEditor;
using Unity.VisualScripting;
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
        public Dictionary<IconType, Image> imageMap { get; set; } = new Dictionary<IconType, Image>();
    }

    [SerializeField] IconImages iconImages;

    List<Text> texts = new List<Text>();
    List<Image> images = new List<Image>();

    List<Graphic> currentTargetGraphics = new List<Graphic>();
    CancellationTokenSource transparentCls = new CancellationTokenSource();
    List<UniTask> fadeTasks = new List<UniTask>();
    private void Start() => Setup();
    public async void ApperUI(MonsterStatusData monsterStatusData,CancellationTokenSource cls)
    {
        GetComponentsInChildren<Graphic>().ToList().ForEach(g => g.gameObject.SetActive(false));
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

        foreach (var keyValuePair in statusDic)
        {
            var type = keyValuePair.Key;
            var content = keyValuePair.Value;
            if(iconImages.imageMap.TryGetValue(type,out var image)) image.gameObject.SetActive(true);
            if (iconImages.textMap.TryGetValue(type, out var text))
            {
                text.gameObject.SetActive(true);    
                text.text = content;
            }
            else continue;
            currentTargetGraphics.Add(text);
            currentTargetGraphics.Add(image);
        }
            //var text = ;
            //var image = images[i];
            //var type = (IconType)i;
            //if(statusDic.TryGetValue(type,out var content)) text.text = content;
            //else
            //{
            //    text.gameObject.SetActive(false);
            //    image.gameObject.SetActive(false);
            //    continue;
            //}
            //var content = content;
           

            //currentTargetGraphics.Add(text);
            //currentTargetGraphics.Add(image);

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
            await fadeTasks;
        }
        catch (OperationCanceledException) { CloseStatusUI(); } 
        fadeTasks.Clear();
        fadeTasks.TrimExcess();
    }
    void Setup()
    {
        var sortedImage = GetComponentsInChildren<Image>().ToList().OrderBy(image => image.transform.GetSiblingIndex()).ToList();
        var sortedTexts = GetComponentsInChildren<Text>().ToList().OrderBy(text => text.transform.GetSiblingIndex()).ToList();
        texts = sortedTexts;
        images = sortedImage;

        var spriteDic = iconImages.iconMap;
        var imageDic = iconImages.imageMap;
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
        for (int i = 0;i < texts.Count; i++)
        {
            var text = texts[i];
            var type = (IconType)i;
            textDic[type] = text;
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


