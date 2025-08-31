using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Threading;

public interface ISide
{
    int ownerID { get; }
}

public class UnitBase : MonoBehaviour, IUnitDamagable,IUnitHealable,IPushable,ISide
{
    public class StatusCondition //:IStatusCondition
    {
        public StatusEffect Paresis { get; set; }
        public StatusEffect BuffSpeed { get; set; }
        public StatusEffect BuffPower { get; set; }
        public StatusEffect DemonCurse { get; set; }
        public StatusEffect Freeze { get; set; }

        public StatusEffect Confusion { get; set; }

        public Dictionary<StatusConditionType,CancellationTokenSource> visualTokens = new Dictionary<StatusConditionType,CancellationTokenSource>();
        public Dictionary<StatusConditionType,GameObject> visualChunks = new Dictionary<StatusConditionType,GameObject>();
        public StatusCondition()
        {
            Paresis = new StatusEffect();
            BuffSpeed = new StatusEffect();
            BuffPower = new StatusEffect();
            DemonCurse = new StatusEffect();
            Freeze = new StatusEffect();
            Confusion = new StatusEffect();
        }
    }
    public float rangeX { get; private set; } = 0f;
    public float rangeZ { get; private set; } = 0f;

    public float prioritizedRange { get; private set; }

    public float originalAnimatorSpeed { get; protected set; } = 0f;
    public MoveType moveType { get; protected set; }
    protected HPbar hPBar = null;
    [SerializeField] public int tentativeID;//これ最終版では消してね
    [SerializeField] UnitScale unitScale;
    [SerializeField] UnitType unitType;
    [SerializeField] List<SkinnedMeshRenderer> mySkinnedMeshes;
    [SerializeField] List<MeshRenderer> myMeshes;
    [SerializeField] Renderer bodyMesh;
    [SerializeField] StatusData statusData;
    public StatusCondition statusCondition { get; private set; }
    public int currentHP { get; protected set;} = 0;
    int maxHP = 0;

    bool isDisplayedHpBar = false;

    public int ownerID { get; set; } = 1;//テスト用に自分から召喚する以外は基本的に相手だから
    public bool isDead { get; set; } = false;

    public MonsterStatusData MonsterStatus
    { 
        get
        {
            if(unitType == UnitType.monster) return StatusData as MonsterStatusData;
            else return null;
        }
    } 

    public FlyingMonsterStatusData FyingMonsterStatus
    {
        get
        {
            if (unitType == UnitType.monster && MonsterStatus.MonsterMoveType == MonsterMoveType.Fly) return StatusData as FlyingMonsterStatusData;
            else return null;
        }
    }

    public ProjectileAttackMonsterStatus ProjectileAttackMonsterStatus 
    {

        get
        {
            if (unitType == UnitType.monster && MonsterStatus.AttackType == AttackType.Long) return StatusData as ProjectileAttackMonsterStatus;
            else return null;
        }
    }

    public RangeAttackMonsterStatusData RangeAttackMonsterStatusData
    {
        get
        {
            if (unitType == UnitType.monster && MonsterStatus.AttackType == AttackType.Range) return StatusData as RangeAttackMonsterStatusData;
            else return null;
        }
    }

    public ContinuousAttackMonsterStatus ContinuousAttackMonsterStatus
    {
        get
        {
            if (unitType == UnitType.monster && MonsterStatus.AttackType == AttackType.Continuous) return StatusData as ContinuousAttackMonsterStatus;
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
    public Vector3 myScale => transform.localScale;

    public UnitType UnitType { get => unitType;}

   public  List<Material[]> meshMaterials { get; protected set; } = new List<Material[]>();
    List<Color[]> originalMaterialColors = new List<Color[]>();

    public bool isKnockBacked_Unit { get; set; } = false;
    public bool isKnockBacked_Spell { get; set; } = false;
    public UnitScale UnitScale { get => unitScale;}
    public Renderer BodyMesh => bodyMesh;

    //protected bool isSettedHPbar { get; private set; } = false;
    //bool test = false;
    //float time = 0f;
    public List<Renderer> AllMesh { get; private set; } = new List<Renderer>();
    protected virtual void Awake()
    {
        Initialize(tentativeID);
        SetRadius();
        prioritizedRange = rangeX >= rangeZ ? rangeX : rangeZ;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        SetRenderers();
        SetMaterialColors();       
        SetHPBar().Forget();
    }
    protected virtual void Update()
    {
        //time += Time.deltaTime;
        //if (time > 3f && !test)
        //{
        //    Test();
        //    test = true;
        //}
        HPBarProcess();
    }

    //public void Test()
    //{
    //    var r = AllMesh[0];
    //    var m = r.material;
    //    var c = m.color;
    //    c.a = 0f;
    //    m.color = c;
    //}
    // 将来はstartで呼ばないから気を付けてね
    public virtual void Initialize(int owner)
    {
       if(owner != -1) SetOwnerID(owner);
       statusCondition = new StatusCondition();
    }
    void SetOwnerID(int owner)
    {
        //ここにPhoton.LocalPlayer.ID...みたいなやつ入れるから将来はこの関数の引数は多分いらない
        ownerID = owner;
        Debug.Log(ownerID);
        //if(ownerID == 0)
        //{
        //    Side = Side.PlayerSide;
        //}
        //else if(ownerID == 1)
        //{
        //    Side = Side.EnemySide;
        //}
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
                //currentHP = currentHP / 2;//テスト用だから後で消してね
                //cmp.ReduceHP(maxHP,currentHP);//後で消してね
                //cmp.gameObject.SetActive(true);//あとで消してね
            }
        }

        //isSettedHPbar = true;
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
    public void Heal(int heal)
    {
        currentHP += heal;
        if (currentHP >= maxHP) currentHP = maxHP;
        hPBar.HealHP(maxHP, currentHP);
    }
    public void EnableHpBar()
    {
       if(hPBar != null) hPBar.gameObject.SetActive(false);
    }
    public virtual void DestroyAll()
    {
        if (hPBar != null) Destroy(hPBar.gameObject);
        if(this != null && this.gameObject != null) Destroy(this.gameObject);
    }

    float GetHPBarOffsetY()
    {
        switch (UnitScale)
        {
            case UnitScale.small:
                return 2.0f;
            case UnitScale.middle:
                return 4.0f;
            case UnitScale.large:
                return 8.0f;
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
                hpBar = await SetFieldFromAssets.SetField<GameObject>("HPBar/Enemy_Middle");
                return hpBar;
            case UnitScale.large:
                hpBar = await SetFieldFromAssets.SetField<GameObject>("HPBar/Enemy_Large");
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

    void SetRenderers()
    {
        if (mySkinnedMeshes.Count != 0)
        {
            mySkinnedMeshes.ForEach(mesh =>
            {
                AllMesh.Add(mesh);
                meshMaterials.Add(mesh.materials);
            });

        }
        if (myMeshes.Count != 0)
        {
            myMeshes.ForEach(mesh =>
            {
                AllMesh.Add(mesh);
                meshMaterials.Add(mesh.materials);
            });
        }
    }
    protected void SetMaterialColors()
    {
        

        foreach (var materials in meshMaterials)
        {
            if (materials.Length == 0) continue;
            var colorArray = new Color[materials.Length];
            for (int i = 0; i < colorArray.Length; i++)
            {
                materials[i].renderQueue = 3001;
                colorArray[i] = materials[i].color;
            }

            originalMaterialColors.Add(colorArray);
        }
    }

    protected async void LitBody()
    {
        var color = Color.red;
        var duration = unitType == UnitType.tower? 0.15f : 0.05f;
        foreach (var materials in meshMaterials)
        {
            foreach(var material in materials)
            {
                var currentAlpha = material.color.a;
                color.a = currentAlpha;
                material.color = color;
            }
        }
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        for (int i = 0; i < meshMaterials.Count;i++)
        {
            for(int j = 0; j < meshMaterials[i].Length;j++)
            {
                var newColor = originalMaterialColors[i][j];
                var currentAlpha = meshMaterials[i][j].color.a;
                newColor.a = currentAlpha;
                meshMaterials[i][j].color = newColor;
            }
        }
    }
    protected void HPBarProcess()
    {
        if (!isDisplayedHpBar && currentHP != maxHP && hPBar != null)
        {
            hPBar.gameObject.SetActive(true);
            hPBar.ReduceHP(maxHP, currentHP);
            isDisplayedHpBar = true;
        }
        else if (isDisplayedHpBar && currentHP == maxHP && hPBar != null)
        {
            hPBar.gameObject.SetActive(false);
            isDisplayedHpBar = false;
        }
    }
}

[Flags]
public enum Side
{
    PlayerSide = 1 << 0,
    EnemySide = 1 << 1,
}
//大きさによってつけるHPバーの大きさを変える
[Flags]
public enum UnitScale
{
    small = 1 << 0,
    middle = 1 << 1,
    large = 1 << 2,
    player = 1 << 3,
    tower = 1 << 4,

    AllExceptTower = player | small | middle | large,
    PlayerAndSmall = player | small,
    PlayerSmallMiddle = player | small | middle
}

public enum UnitType
{
    monster,
    tower,
    player,
}
