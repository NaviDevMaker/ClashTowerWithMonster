using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace Game.Monsters
{
    public class MultiSpawnMonster : MonoBehaviour, IMonster,ISummonbable,ISide
    {
        [SerializeField] int tentativeID;
        [SerializeField] MonsterStatusData monsterStatusData;
        [SerializeField] protected MultiSpawnMonsterData multiSpawnMonsterData;
        public bool isSummonedInDeckChooseScene { get => throw new System.NotImplementedException();
                                                  set => throw new System.NotImplementedException(); }
        public MonsterStatusData _MonsterStatus => monsterStatusData;

        public FlyingMonsterStatusData _FlyingMonsterStatus
        {
            get
            {
                if (monsterStatusData.MonsterMoveType == MonsterMoveType.Fly) return monsterStatusData as FlyingMonsterStatusData;
                else return null;
            }
        }
        public ProjectileAttackMonsterStatus _ProjectileAttackMonsterStatus
        {
            get
            {
                if (monsterStatusData.AttackType == AttackType.Long) return monsterStatusData as ProjectileAttackMonsterStatus;
                else return null;
            }
        }
        public RangeAttackMonsterStatusData _RangeAttackMonsterStatus
        {
            get
            {
                if (monsterStatusData.AttackType == AttackType.Range) return monsterStatusData as RangeAttackMonsterStatusData;
                else return null;
            }
        }
        public ContinuousAttackMonsterStatus _ContinuousAttackMonsterStatus
        {
            get
            {
                if (monsterStatusData.AttackType == AttackType.Continuous) return monsterStatusData as ContinuousAttackMonsterStatus;
                else return null;
            }
        }
        public FlyProjectileStatusData _FlyProjectileAttackMonsterStatus
        {
            get
            {
                if (monsterStatusData.MonsterMoveType == MonsterMoveType.Fly 
                    && monsterStatusData.AttackType == AttackType.Range) return monsterStatusData as FlyProjectileStatusData;
                else return null;
            }
        }

        public UnitType _UnitType => UnitType.monster;
        public Renderer _BodyMesh => throw new System.NotImplementedException();
        public int _SpawnCount => multiSpawnMonsterData.SpawnCount;
        public int ownerID { get; set; } = 1;
        public bool isSummoned { get; set; }
        public string SummonedCardName { get; set; }


        private void Awake() { ownerID = tentativeID; }
        protected virtual void Start() {}
        public virtual async void SpawnMonsters(Vector3 pos, Quaternion rot, UnityAction<IMonster,bool> alphaChange)
        {
            await UniTask.CompletedTask;
        }
    }
}


