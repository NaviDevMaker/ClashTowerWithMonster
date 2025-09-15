using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Monsters.Archer;
using System;

public interface IBuilding { }
public interface ILongDistanceAttacker<T> where T : UnitBase
{
    List<LongDistanceAttack<T>> movers { get; set; }
    Transform startTra { get;}
    void EndMoveAction(LongDistanceAttack<T> mover);
    int moverCount { get;}
    void SetMoverToList();
}


public class TowerController :UnitBase,IBuilding,ILongDistanceAttacker<TowerController>
{
    [SerializeField] ArcherController archer;
    UnitBase targetEnemy;
    
    public int moverCount { get; private set;} = 5;
    public List<LongDistanceAttack<TowerController>> movers { get; set; } = new List<LongDistanceAttack<TowerController>>();
    public Transform startTra { get => throw new NotImplementedException();}

    float deathActionLength = 0f;
    bool isSettedLength = false;
    enum State
    {
       Search,
       Stay,
       Death,
    }

    State state;
    protected override void Start()
    {
        moveType = MoveType.Walk;
        base.Start();
        //Initialize(ownerID);
        //Debug.Log(Side);
        state = State.Search;
        SetMaterialRendererFace();
    }

    protected override void Update()
    {
        if(isDead && state != State.Death)
        {
            state = State.Death;
            DeathAction();
        }
        base.Update();
          
        ChangeEnemy();
        switch (state)
        {
            case State.Search:
                //isFirstShot = false;//敵が範囲内に誰もいなくなったとき、入ってきた瞬間に一発目を打てるようにここでfalseにする
                SearchEnemy();break;
            case State.Stay:
            break;
        }

    }
    void SearchEnemy()
    {
  
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(this.gameObject,TowerStatus.SearchRadius);

        sortedArray.ToList().ForEach((c) => Debug.Log(c.gameObject.name));
        if (sortedArray != null)
        {
            foreach (var hit in sortedArray)
            {
                var hitEnemyType = hit.GetUnitSide(ownerID);
                if(hit is IMonster)
                {
                    if(hit.TryGetComponent<ISummonbable>(out var summonbable))
                    {
                        var summoned = summonbable.isSummoned;
                        if (hit.gameObject == this.gameObject || hit.isDead || hitEnemyType == Side.PlayerSide || !summoned) continue;
                    }
                }
                var isTransparent = hit.statusCondition.Transparent.isActive;
                var isNonTarget = hit.statusCondition.NonTarget.isActive;
                if (hit.gameObject == this.gameObject || hit.isDead || hitEnemyType == Side.PlayerSide
                    || isTransparent || isNonTarget) continue;
                targetEnemy = hit;
                archer.target = hit;
                Debug.Log("敵を発見しました");
                state = State.Stay;
                Debug.Log("最初の矢が放たれました");
                break;
            }
        }
        else state = State.Search;
    }

    void ChangeEnemy()
    {
        if (targetEnemy == null) return;
        var sortedArray = SortExtention.GetSpecificColliderInRange<UnitBase>(this,TowerStatus.SearchRadius);
        bool stillInRange = false;
        bool isDeadTarget = targetEnemy.isDead;
        foreach (Collider col in sortedArray)
        {
            if(col.gameObject == targetEnemy.gameObject)
            {
               stillInRange = true;
               break;
            }
        }
        if(!stillInRange || isDeadTarget)
        {
            targetEnemy = null;
            archer.target = null;
            state = State.Search;
            Debug.Log("ターゲットが範囲外になりました");
        }
    }

    public void SetMoverToList()
    {
        var parent = archer.arrowHand;
        var pos = archer.originalArrowPos;
        var rot = archer.originalArrowRot;
        //gunParent = new GameObject("GunParent");
        for (int i = 0; i < moverCount; i++)
        {
            var gunMover = Instantiate(TowerStatus.TowerShotgun,Vector3.zero,Quaternion.identity);
            gunMover.Setup(this,parent,pos,rot,movers,EndMoveAction,TowerStatus.GunSpeed);
            //GunSetUp(gunMover);
        }

        var castedShotGuns = this.movers.OfType<GunMover>().ToList();
        archer.shotGuns = castedShotGuns;
    }
    public void EndMoveAction(LongDistanceAttack<TowerController> gun)
    {
       // Debug.Log("定位置に戻ります");
        gun.gameObject.SetActive(false);
        gun.transform.SetParent(archer.arrowHand);
        gun.gameObject.transform.localPosition = archer.originalArrowPos;
        gun.transform.localRotation = archer.originalArrowRot;
        gun.IsReachedTargetPos = false;
        gun.target = null;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, TowerStatus.SearchRadius);
        Gizmos.color = Color.cyan;
        DrawEllipse(transform.position, rangeX, rangeZ, 32);
    }

    void DrawEllipse(Vector3 center, float radiusX, float radiusZ, int segments)
    {
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float x = Mathf.Cos(angle) * radiusX;
            float z = Mathf.Sin(angle) * radiusZ;
            Vector3 nextPoint = new Vector3(x, 0, z) + center;

            if (i > 0)
            {
                Gizmos.DrawLine(prevPoint, nextPoint);
            }
            prevPoint = nextPoint;
        }
    }
    public override void Initialize(int owner = -1)
    {
        base.Initialize(owner);
        archer.OnDestoryedTower += SetLength;
        SetMoverToList();
        archer.shotDuration = TowerStatus.ShotDuration;
    }

    public override void Damage(int damage)
    {
        base.Damage(damage);
    }

    async void DeathAction()
    {
        archer.isDestroyedTower = true;
        await UniTask.WaitUntil(() => isSettedLength);
        this.ExecuteDeathAction_Tower(deathActionLength).Forget();
        var timeScaleAmount = 0.25f;
        Time.timeScale = timeScaleAmount;
        var delay = 0.5f;
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        Time.timeScale = 1f;
    }
    void SetLength(float animLength, float animSpeed)
    {
        var length  = animLength * animSpeed;
        deathActionLength = length;
        isSettedLength = true;
    }
    public override void DestroyAll()
    {
        base.DestroyAll();
        if(archer != null) Destroy(archer.gameObject);
    }

    void SetMaterialRendererFace()
    {
        var _meshMaterials = meshMaterials[0];
        var frontIndex = new int[] {0,1,2,3,6};
        var backIndex = new int[] { 4,5 };
        for (int i = 0; i < _meshMaterials.Length; i++)
        {
            var m = _meshMaterials[i];
            Debug.Log($"{m.name}変更します");
            if(frontIndex.Contains(i))
            {
                m.SetInt("_Cull", 2); // 両面表示
            }
            else if(backIndex.Contains(i))
            {
                m.SetInt("_Cull", 1); // 両面表示
            }

            //これないとダメっぽい
            m.SetInt("_ZWrite", 1);
            m.SetInt("_ZTest", 4);
            m.SetInt("_SrcBlend", 1);
            m.SetInt("_DstBlend", 0);
            m.renderQueue = 2000;
        }

        meshMaterials[0] = _meshMaterials;
    }
}
