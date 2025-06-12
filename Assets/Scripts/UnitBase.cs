using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Net.NetworkInformation;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEditor.Build;
using System.Collections.Generic;
using System.Linq;
using System;
public class UnitBase : MonoBehaviour, IUnitDamagable,IPushable
{

    public float rangeX { get; private set; } = 0f;
    public float rangeZ { get; private set; } = 0f;

    public float prioritizedRange { get; private set; }

    public MoveType moveType { get; protected set; }
    HPbar hPBar = null;
    [SerializeField] UnitScale unitScale;
    [SerializeField] UnitType unitType;
    [SerializeField] List<SkinnedMeshRenderer> mySkinnedMeshes;
    [SerializeField] List<MeshRenderer> myMeshes;
    [SerializeField] StatusData statusData;
    int currentHP = 0;
    int maxHP = 0;

    bool isDisplayedHpBar = false;

    //攻撃時に攻撃側がさんしょうするためpublic
    public Side Side;
    //
    public int ownerID = -1;
    public bool isDead { get; private set; } = false;
    public List<SkinnedMeshRenderer> MySkinnedMeshes { get => mySkinnedMeshes;}
    public List<MeshRenderer> MyMeshes { get => myMeshes;}
    public MonsterStatusData MonsterStatus
    { 
        get
        {
            if(unitType == UnitType.monster) return StatusData as MonsterStatusData;
            else return null;
        }
    } 
    public TowerStatusData TowerStatus
    {
        get
        {
            if (unitType == UnitType.tower) return StatusData as TowerStatusData;
            else return null;
        }
    }

    public PlayerStatusData PlayerStatus
    {
        get
        {
            if(unitType == UnitType.player) return StatusData as PlayerStatusData;
            else return null;
        }
    }

    public StatusData StatusData { get => statusData;}
    public UnitScale UnitScale { get => unitScale;}
    public Vector3 myScale => transform.localScale;

    public UnitType UnitType { get => unitType;}

    List<Material[]> meshMaterials = new List<Material[]>();
    List<Color[]> originalMaterialColors = new List<Color[]>();

    public bool isKnockBacked { get; set; } = false;
    protected virtual void Awake()
    {
        SetRadius();
        prioritizedRange = rangeX >= rangeZ ? rangeX : rangeZ;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        SetMaterialColors();       
        SetHPBar().Forget();
        Initialize(ownerID);
    }

    protected virtual void Update()
    {
        if(!isDisplayedHpBar && currentHP != maxHP && hPBar!= null)
        {
            hPBar.gameObject.SetActive(true);
            hPBar.ReduceHP(maxHP, currentHP);
            isDisplayedHpBar=true;
        }
    }

    //将来プレイヤー側から呼ぶためにpublic
    public virtual void Initialize(int owner)
    {

       if(owner != -1) SetUnitSide(owner);
    }
    void SetUnitSide(int owner)
    {
        ownerID = owner;
        if(ownerID == 0)
        {
            Side = Side.PlayerSide;
        }
        else if(ownerID == 1)
        {
            Side = Side.EnemySide;
        }
    }
    async UniTask SetHPBar()
    {
        var offsetY = GetHPBarOffsetY(); ;
        var _hpBar = await GetHPBar();
        
        if (_hpBar != null)
        {
            _hpBar.SetActive(false);
            var bar = Instantiate(_hpBar);
            var pos = transform.position + Vector3.up * offsetY;
            bar.transform.position = pos;
            var cmp = bar.GetComponent<HPbar>();
            if (cmp != null)
            {
                hPBar = cmp;
                cmp.chaseUnit = this.gameObject;
                cmp.offsetY = offsetY;
                currentHP = StatusData.Hp;
                maxHP = currentHP;
            }

        }
    }
    public virtual void Damage(int damage) 
    {
        currentHP -= damage;
       
        Debug.Log($"{name}は{damage}ダメーじ、残り{currentHP}");
        hPBar.ReduceHP(maxHP, currentHP);//テスト用にコメントアウトしたから後で戻して
        if (currentHP <= 0)
        {
            Debug.Log("死にました");
            isDead = true;
            return;
        }

        LitBody();
    }
    public void EnableHpBar()
    {
        hPBar.gameObject.SetActive(false);
    }
    public void DestroyAll()
    {
        Destroy(this.gameObject);
        Destroy(hPBar.gameObject);
    }

    float GetHPBarOffsetY()
    {
        switch (UnitScale)
        {
            case UnitScale.small:
                return 2.0f;
            case UnitScale.middle:
            case UnitScale.large:
                return 1.0f;
            case UnitScale.tower:
                return 12.0f;
            case UnitScale.player:
                return 4.0f;
            default:
                return 1.0f;
        }
    }

    async UniTask<GameObject> GetHPBar()
    {
        GameObject hpBar = null;
        switch (UnitScale)
        {
            case UnitScale.small:
               hpBar = await SetFieldFromAssets.SetField<GameObject>("HPBar/Enemy_Small");
                return hpBar;
            case UnitScale.middle:
            case UnitScale.large:
                return hpBar;
            case UnitScale.tower:
                hpBar = await SetFieldFromAssets.SetField<GameObject>("HPBar/Tower");
                return hpBar;
            case UnitScale.player:
                hpBar = await SetFieldFromAssets.SetField<GameObject>("HPBar/Player");
                return hpBar;
            default:
                return hpBar;
        }
    }

    void SetRadius()
    {
       var collider = GetComponent<Collider>();
        if (collider == null) return;
        Debug.Log(gameObject.name);
        var bounds = collider.bounds;
        rangeX = bounds.extents.x;
        rangeZ = bounds.extents.z;
    }
    void SetMaterialColors()
    {
        if (MySkinnedMeshes.Count != 0) MySkinnedMeshes.ForEach(mesh => meshMaterials.Add(mesh.materials));
        if (MyMeshes.Count != 0) MyMeshes.ForEach(mesh => meshMaterials.Add(mesh.materials));
 
        foreach (var materials in meshMaterials)
        {
            if (materials.Length == 0) continue;
            var colorArray = new Color[materials.Length];
            for (int i = 0; i < colorArray.Length; i++)
            {
                colorArray[i] = materials[i].color;
            }

            originalMaterialColors.Add(colorArray);
        }
    }

    protected async void LitBody()
    {
        var color = Color.red;
        var duration = 0.05f;
        foreach (var materials in meshMaterials)
        {
            foreach(var material in materials)
            {
                material.color = color;
            }
        }
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        for (int i = 0; i < meshMaterials.Count;i++)
        {
            for(int j = 0; j < meshMaterials[i].Length;j++)
            {
                meshMaterials[i][j].color = originalMaterialColors[i][j];
            }
        }
    }

    internal bool TryGetComponent(Type type, out IUnitDamagable damageable)
    {
        throw new NotImplementedException();
    }
}

public enum Side
{
    PlayerSide,
    EnemySide,
}
//大きさによってつけるHPバーの大きさを変える
public enum UnitScale
{
    small,
    middle,
    large,
    player,
    tower,
}

public enum UnitType
{
    monster,
    tower,
    player,
}
