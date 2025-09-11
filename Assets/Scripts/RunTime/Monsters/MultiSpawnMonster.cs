using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;
using Game.Monsters.Fishman;
using System;

public interface ISpawner
{
    void SpawnMonsters(Vector3 pos, Quaternion rot, UnityAction<IMonster, bool> alphaChange);
    int _SpawnCount {get;}
    Transform parent { get; }
}

namespace Game.Monsters
{
    public class MultiSpawnMonster<T> : MonoBehaviour, IMonster,ISummonbable,ISide,ISpawner where T : MonsterControllerBase<T>
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
        public Transform parent => transform;
        public int ownerID { get; set; } = 1;
        public bool isSummoned { get; set; }
        public string SummonedCardName { get; set; }


        private void Awake() { ownerID = tentativeID; }
        protected virtual void Start() {}
        public virtual async void SpawnMonsters(Vector3 pos, Quaternion rot, UnityAction<IMonster,bool> alphaChange)
        {
            var prefab = multiSpawnMonsterData.MonsterPrefab;
            var delayTime = multiSpawnMonsterData.EachSpawnDelayTime;
            var positions = GetPositions(pos);
            for (int i = 0; i < multiSpawnMonsterData.SpawnCount; i++)
            {
                var spawnPos = positions[i];
                var monster = Instantiate(prefab, spawnPos, rot);
                var monsterInterFace = monster.GetComponent<IMonster>();
                alphaChange?.Invoke(monsterInterFace, true);

                //‚±‚±•’Ê‚É‚¢‚ç‚ñ‚©‚àAPhoton‚¾‚ÆCreate‚È‚ñ‚¿‚á‚ç‚ÅInstaciate‚ğ’N‚ª‚µ‚½‚©‚Ìî•ñ‚Á‚Ä‚é‚ç‚µ‚¢‚©‚ç
                var cmp = monster.GetComponent<T>();
                if (cmp != null)
                {
                    cmp.ownerID = ownerID;
                    cmp.isSummoned = true;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            if (this != null) Destroy(this.gameObject);
        }
        protected virtual Vector3[] GetPositions(Vector3 pos) => default;
    }
}


