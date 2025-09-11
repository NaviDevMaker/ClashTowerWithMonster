using Game.Monsters;
using Game.Monsters.BlackKnight;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public interface IRangeAttackAddForceBase { }
public interface IRangeWeponAddForce<T> : IRangeAttackAddForceBase where T : MonsterControllerBase<T>
{
    AddForceToUnit<PushableWeponBase<T>> _addForceToUnit {get;}
    void PushUnit(UnitBase target);
}

public class PushableWeponBase<TOwner> : MonoBehaviour,IPushable, ISide, IMonster
    where TOwner : MonsterControllerBase<TOwner>
{
    protected TOwner weponOwner;
    public float rangeX { get; private set; }
    public float rangeZ { get; private set; }
    public float prioritizedRange { get; private set; }
    public bool isKnockBacked_Unit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool isKnockBacked_Spell { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public MoveType moveType { get;protected set; }
    public UnitScale UnitScale { get; protected set; }
    public int ownerID { get; set; }
    public bool isSummonedInDeckChooseScene { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public MonsterStatusData _MonsterStatus => throw new System.NotImplementedException();
    public FlyingMonsterStatusData _FlyingMonsterStatus => throw new System.NotImplementedException();
    public ProjectileAttackMonsterStatus _ProjectileAttackMonsterStatus => throw new System.NotImplementedException();
    public RangeAttackMonsterStatusData _RangeAttackMonsterStatus => throw new System.NotImplementedException();
    public UnitType _UnitType => throw new System.NotImplementedException();
    public Renderer _BodyMesh => throw new System.NotImplementedException();

    public ContinuousAttackMonsterStatus _ContinuousAttackMonsterStatus => throw new System.NotImplementedException();

    public FlyProjectileStatusData _FlyProjectileAttackMonsterStatus => throw new System.NotImplementedException();

    
    public virtual void Initialize(TOwner owner)
    {
        this.weponOwner = owner;
        if (TryGetComponent<SphereCollider>(out var sphereCollider))
        {
            var scaleAmount = transform.lossyScale;
            Debug.Log(scaleAmount);
            var colliderRangeProvider = new SphereColliderRangeProvider
            {
                sphereCollider = sphereCollider
            };
            rangeX = colliderRangeProvider.GetRangeX() * scaleAmount.x;
            rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount.z;
            var prioAmount = Mathf.Max(scaleAmount.x, scaleAmount.z);
            prioritizedRange = colliderRangeProvider.GetPriorizedRange() * prioAmount;
        }
        else if (TryGetComponent<BoxCollider>(out var boxCollider))
        {
            var scaleAmount = transform.lossyScale.magnitude;
            var colliderRangeProvider = new BoxColliderrangeProvider
            {
                boxCollider = boxCollider
            };
            rangeX = colliderRangeProvider.GetRangeX();
            rangeZ = colliderRangeProvider.GetRangeZ();
            prioritizedRange = colliderRangeProvider.GetPriorizedRange();
        }
    }
}
