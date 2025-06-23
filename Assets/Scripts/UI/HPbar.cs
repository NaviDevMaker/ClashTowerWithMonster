using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System;
public class HPbar : MonoBehaviour
{
    new GameObject camera;
    [SerializeField] Image hpImage;
    float time = 0f;
    public float offsetY{ get; set; } 
    public GameObject chaseUnit { get; set;}
    Color originalColor = default;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(ColorUtility.TryParseHtmlString("#34FF4A",out var color)) originalColor = color;
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

    //void LookToCamera()
    //{
    //    var direction = camera.transform.position - transform.position;

    //    var projected = Vector3.ProjectOnPlane(direction, transform.right);
    //    Quaternion rot = Quaternion.LookRotation(projected, transform.up);
    //    transform.rotation = rot;
    //}

    public async void LitBar()
    {
        Debug.Log("a");
        var duration = 0.01f;
        var color = Color.white;
        hpImage.color = color;
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        hpImage.color = originalColor;

    }
    void ChaseUnit()
    {
        var pos = chaseUnit.transform.position + Vector3.up * offsetY;
        transform.position = pos;
    }

    public void ReduceHP(int maxHP,int currentHP)
    {
        Debug.Log("å∏ÇÁÇµÇ‹Ç∑");
        hpImage.fillAmount = (float)currentHP / (float)maxHP;
        LitBar();
    }

    public void HealHP(int maxHP,int currentHP)
    {
        Debug.Log("ëùÇ‚ÇµÇ‹Ç∑");
        if(currentHP == maxHP) return;
        hpImage.fillAmount = (float)currentHP /(float)maxHP;
        LitBar();
    }


}
