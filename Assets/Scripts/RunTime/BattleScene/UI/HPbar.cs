using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
public class HPbar : MonoBehaviour
{
    new GameObject camera;
    [SerializeField] Image hpImage;
    [SerializeField] Image shadowImage;
    public float offsetY{ get; set; } 
    public GameObject chaseUnit { get; set;}
    Color originalForwardColor = default;
    Color originalShadowColor = default;

    private void Awake()
    {
        if (ColorUtility.TryParseHtmlString("#34FF4A", out var forwardColor)) originalForwardColor = forwardColor;
        if (ColorUtility.TryParseHtmlString("#000000", out var shadowColor))
        {
            var translusent = 140.0f / 250.0f;
            shadowColor.a = translusent;
            originalShadowColor = shadowColor;
        }
        camera = Camera.main.gameObject;
    }
    // Update is called once per frame
    void Update()
    {
        UIFuctions.LookToCamera(this.gameObject);// LookToCamera();
    }
    private void LateUpdate()
    {
        ChaseUnit();
    }

    public void LitBar()
    {
        //Debug.Log("a");
         hpImage.LitBar(originalForwardColor).Forget();
         shadowImage.LitBar(originalShadowColor).Forget();
    }
    void ChaseUnit()
    {
        if (chaseUnit == null) return;
        var pos = chaseUnit.transform.position + Vector3.up * offsetY;
        transform.position = pos;
    }
    public void ReduceHP(int maxHP,int currentHP)
    {
        Debug.Log("å∏ÇÁÇµÇ‹Ç∑");
        hpImage.fillAmount = (float)currentHP / (float)maxHP;
        shadowImage.fillAmount = (float)currentHP / (float)maxHP;
        LitBar();
    }
    public void HealHP(int maxHP,int currentHP)
    {
        Debug.Log("ëùÇ‚ÇµÇ‹Ç∑");
        if(currentHP == maxHP) return;
        hpImage.fillAmount = (float)currentHP /(float)maxHP;
        shadowImage.fillAmount = (float) currentHP / (float)maxHP;
        LitBar();
    }
}
