using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Monsters.Archer;
using System;
using UnityEngine.Events;

public interface IBuilding { }
public interface ILongDistanceAttacker<T> where T : UnitBase
{
    List<LongDistanceAttack<T>> movers { get; set; }
    Transform startTra { get;}
    void EndMoveAction(LongDistanceAttack<T> mover);
    int moverCount { get;}
    void SetMoverToList();
}

public interface ITower { }


public class TowerController :UnitBase,IBuilding,ILongDistanceAttacker<TowerController>,ITower
{
    [SerializeField] ArcherController archer;
    UnitBase targetEnemy;
    
    public int moverCount { get; private set;} = 5;
    public List<LongDistanceAttack<TowerController>> movers { get; set; } = new List<LongDistanceAttack<TowerController>>();
    public Transform startTra { get => throw new NotImplementedException();}
    public SimpleTowerStatus SimpleTowerStatus => StatusData as SimpleTowerStatus;
    float deathActionLength = 0f;
    bool isSettedLength = false;

    public UnityEvent<TowerController> towerHpUIEvent;
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
                //isFirstShot = false;//�G���͈͓��ɒN�����Ȃ��Ȃ����Ƃ��A�����Ă����u�ԂɈꔭ�ڂ�łĂ�悤�ɂ�����false�ɂ���
                SearchEnemy();break;
            case State.Stay:
            break;
        }

    }
    void SearchEnemy()
    {
        var sortedArray = SortExtention.GetSortedArrayByDistance_Sphere<UnitBase>(this.gameObject,SimpleTowerStatus.SearchRadius);

        sortedArray.ToList().ForEach((c) => Debug.Log(c.gameObject.name));
        if (sortedArray.Length != 0)
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
                Debug.Log("�G�𔭌����܂���");
                state = State.Stay;
                Debug.Log("�ŏ��̖������܂���");
                break;
            }
        }
        else state = State.Search;
    }

    void ChangeEnemy()
    {
        if (targetEnemy == null) return;
        var sortedArray = SortExtention.GetSpecificColliderInRange<UnitBase>(this,SimpleTowerStatus.SearchRadius);

        bool stillInRange = false;
        bool isDeadTarget = targetEnemy.isDead;
        bool isTransparent = targetEnemy.statusCondition.Transparent.isActive;
        bool isNonTarget = targetEnemy.statusCondition.NonTarget.isActive;

        foreach (Collider col in sortedArray)
        {
            if(col.gameObject == targetEnemy.gameObject)
            {
               stillInRange = true;
               break;
            }
        }
        if(!stillInRange || isDeadTarget || targetEnemy is IInvincible invincible && invincible.IsInvincible
            || isTransparent || isNonTarget)
        {
            targetEnemy = null;
            archer.target = null;
            state = State.Search;
            Debug.Log("�^�[�Q�b�g���͈͊O�ɂȂ�܂���");
        }
    }

    public void SetMoverToList()
    {
        var parent = archer.arrowHand;
        var pos = archer.originalArrowPos;
        var rot = archer.originalArrowRot;
        for (int i = 0; i < moverCount; i++)
        {
            var gunMover = Instantiate(SimpleTowerStatus.GunMover,Vector3.zero,Quaternion.identity);
            gunMover.Setup(this,parent,pos,rot,movers,EndMoveAction,SimpleTowerStatus.MoverSpeed,SimpleTowerStatus.AttackAmount);
            //GunSetUp(gunMover);
        }

        var castedShotGuns = this.movers.OfType<GunMover>().ToList();
        archer.shotGuns = castedShotGuns;
    }
    public void EndMoveAction(LongDistanceAttack<TowerController> gun)
    {
       // Debug.Log("��ʒu�ɖ߂�܂�");
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
        Gizmos.DrawWireSphere(transform.position,SimpleTowerStatus.SearchRadius);
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
        archer.shotDuration = SimpleTowerStatus.ShotDuration;
    }

    public override void Damage(int damage)
    {
        base.Damage(damage);
        towerHpUIEvent.Invoke(this);
    }

    public override void Heal(int heal)
    {
        base.Heal(heal);
        towerHpUIEvent.Invoke(this);
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
            Debug.Log($"{m.name}�ύX���܂�");
            if(frontIndex.Contains(i))
            {
                m.SetInt("_Cull", 2); // ���ʕ\��
            }
            else if(backIndex.Contains(i))
            {
                m.SetInt("_Cull", 1); // ���ʕ\��
            }

            //����Ȃ��ƃ_�����ۂ�
            m.SetInt("_ZWrite", 1);
            m.SetInt("_ZTest", 4);
            m.SetInt("_SrcBlend", 1);
            m.SetInt("_DstBlend", 0);
            m.renderQueue = 2000;
        }

        meshMaterials[0] = _meshMaterials;
    }
}
