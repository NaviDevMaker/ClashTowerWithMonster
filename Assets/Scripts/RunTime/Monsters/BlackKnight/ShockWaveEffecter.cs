using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

public interface IRangeWeponAttack { }
namespace Game.Monsters.BlackKnight
{
    public class ShockWaveEffecter : MonoBehaviour, IPushable, ISide,IMonster,IRangeWeponAttack
    {
        public float rangeX { get;private set;}
        public float rangeZ { get;private set;}
        public float prioritizedRange { get;private set;}
        public bool isKnockBacked_Unit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool isKnockBacked_Spell { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public MoveType moveType => MoveType.Walk;
        public UnitScale UnitScale => UnitScale.middle;
        public int ownerID { get; set; }
        public bool isSummonedInDeckChooseScene { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public MonsterStatusData _MonsterStatus => throw new System.NotImplementedException();
        public FlyingMonsterStatusData _FlyingMonsterStatus => throw new System.NotImplementedException();
        public ProjectileAttackMonsterStatus _ProjectileAttackMonsterStatus => throw new System.NotImplementedException();
        public RangeAttackMonsterStatusData _RangeAttackMonsterStatus => throw new System.NotImplementedException();
        public UnitType _UnitType => throw new System.NotImplementedException();
        public Renderer _BodyMesh => throw new System.NotImplementedException();

        public AddForceToUnit<ShockWaveEffecter> AddForceToUnit { get; private set; }

        public ContinuousAttackMonsterStatus _ContinuousAttackMonsterStatus => throw new System.NotImplementedException();

        public void Initialize(float pushAmount,float pushDuration,int ownerID)
        {
            this.ownerID = ownerID;
            if (TryGetComponent<SphereCollider>(out var sphereCollider))
            {
                var scaleAmount = transform.lossyScale.magnitude;
                var colliderRangeProvider = new SphereColliderRangeProvider
                {
                    sphereCollider = sphereCollider
                };
                rangeX = colliderRangeProvider.GetRangeX() * scaleAmount;
                rangeZ = colliderRangeProvider.GetRangeZ() * scaleAmount;
                prioritizedRange = colliderRangeProvider.GetPriorizedRange() * scaleAmount;
                AddForceToUnit = new AddForceToUnit<ShockWaveEffecter>(this, pushAmount, pushDuration, PushEffectUnit.OnlyEnemyUnit);
            }
        }

        public void SetHitJudgementObject() => throw new System.NotImplementedException();
    }
}

