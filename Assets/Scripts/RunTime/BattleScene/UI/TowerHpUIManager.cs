using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
//ActorNumberÇ¡ÇƒÇ‚Ç¬Ç≈ä«óùÇ∑ÇÈÇ±Ç∆Ç…ÇµÇΩÇ©ÇÁîÒìØä˙Ç≈NumberÇ™êUÇËï™ÇØÇÁÇÍÇÈÇ‹Ç≈ë“Ç¬ïKóvÇÕÇ»Ç¢ÇÊ
public class TowerHpUIManager : MonoBehaviour
{
    List<TowerController> myTowerList = new List<TowerController>();
    [SerializeField] List<Image> parentImages = new List<Image>();
    Dictionary<TowerController,Image> imageDic = new Dictionary<TowerController,Image>();
    Dictionary<TowerController,Text> hpTextDic = new Dictionary<TowerController, Text>();
    Dictionary<TowerController,(Text deadText,bool isAppear)> deadTextDic = new Dictionary<TowerController,(Text,bool)>();

    Color firstColor;
    Color middleColor;
    Color dangerColor;

    private void Start()
    {
        ColorGetByHp();
        GetMyTower();
    }
    void GetMyTower()
    {
        var towers = FindObjectsByType<TowerController>(sortMode:FindObjectsSortMode.None);
        towers.ToList().ForEach(tower =>
        {
            if (tower.ownerID != 0) return;
            myTowerList.Add(tower);
        });

        for (int i = 0; i < myTowerList.Count; i++)
        {
            var tower = myTowerList[i];  
            var parentImage = parentImages[i];
            var image = parentImage.transform.GetChild(0).GetComponent<Image>();
            var hpText = parentImage.transform.GetChild(1).GetComponent<Text>();
            var deadText = parentImage.transform.GetChild(2).GetComponent<Text>();
            imageDic[tower] = image;
            hpTextDic[tower] = hpText;
            deadTextDic[tower] = (deadText,false);
            var hp = tower.TowerStatus.Hp;
            hpText.text = hp.ToString();
            tower.towerHpUIEvent.AddListener(RenewUI);
        }
    }
    void RenewUI(TowerController tower)
    {
        var maxHP = tower.TowerStatus.Hp;
        var currentHP = tower.currentHP;
        var fill = (float)currentHP/(float)maxHP;
        var targetImageParent = imageDic[tower];
        var currentColor = GetCurrentHPColor(fill);
        targetImageParent.color = currentColor;
        targetImageParent.LitBar(currentColor);
        targetImageParent.fillAmount = fill;
        var hpText = hpTextDic[tower];
        var hpAmount = Mathf.Max(0, currentHP);
        if (hpAmount <= 0)
        {  
            var isAppear = deadTextDic[tower].isAppear;
            if (isAppear) return;
            var duration = 1.0f;
            var deadText = deadTextDic[tower].deadText;
            var scaleSet = new Vector3TweenSetup(Vector3.one * 1.3f,duration / 2);
            var scaleSet_Original = new Vector3TweenSetup(Vector3.one, duration / 2);
            var scaleTween = deadText.gameObject.Scaler(scaleSet);
            var scaleTween_Original = deadText.gameObject.Scaler(scaleSet_Original);
            var seq = DOTween.Sequence().Append(scaleTween)
                      .Append(scaleTween_Original);
            deadTextDic[tower] = (deadText,true);
            hpAmount = 0;
        }
        hpText.text = hpAmount.ToString();
    }
    void ColorGetByHp()
    {
        if(ColorUtility.TryParseHtmlString("#34FF4A",out var firstColor)) this.firstColor = firstColor;
        if(ColorUtility.TryParseHtmlString("#FFD934",out var middleColor)) this.middleColor = middleColor;
        if(ColorUtility.TryParseHtmlString("#FF3434",out var dangerColor)) this.dangerColor = dangerColor;
    }
    Color GetCurrentHPColor(float fillAmount)
    {
        return fillAmount switch
        {
             >= 0.7f => firstColor,
             >= 0.3f => middleColor,
             _=> dangerColor,
        };       
    }
}
