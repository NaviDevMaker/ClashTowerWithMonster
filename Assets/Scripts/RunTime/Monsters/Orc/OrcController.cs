using Game.Monsters.BlackKnight;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Game.Monsters.Orc
{
    public class OrcController : MonsterControllerBase<OrcController>,IRangeAttack, IRepeatAttack
    {
        [System.Serializable]
        public class BombInfo
        {
            [SerializeField] public OrcBomb spawnedBomb;
            [SerializeField] public int bombDamage;
            public readonly float spawnInterval = 3.0f;
            public readonly float offsetZ = 1.5f;
            public int direction = 1;
            public float elapsedTime { get; set;} = 0f;
        }

        [SerializeField] public BombInfo bombInfo;
        public GameObject rangeAttackObj { get; set; }
        public OrcWeponPusher orcWeponPusher { get; private set;}
        public int repeatCount => 3;

        protected override void Awake()
        {
            base.Awake();
            isSummoned = true;//テスト用だから消して
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            SetHitJudgementObject();
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
            bombInfo.elapsedTime += Time.deltaTime;
            if(!isDead && bombInfo.elapsedTime >= bombInfo.spawnInterval)
            {
                bombInfo.elapsedTime = 0f;
                DropBomb();
            }
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
        public void SetHitJudgementObject()
        {
            var data = _RangeAttackMonsterStatus;
            if (data == null) return;
            var weponName = data._RangeAttackInfo.RangeAttackWepon.name;
            rangeAttackObj = this.gameObject.GetObject(weponName);
            orcWeponPusher = rangeAttackObj.AddComponent<OrcWeponPusher>();
            orcWeponPusher.Initialize(this);
            Debug.Log(rangeAttackObj.name);
        }
        void DropBomb()
        {
            var offsetZ = -(transform.forward * 1.5f);
            var offsetX = transform.right * 0.5f * bombInfo.direction;
            var spawnPos = transform.position + offsetZ + offsetX;
            var prefab = bombInfo.spawnedBomb;
            var bomb = Instantiate(prefab, spawnPos, Quaternion.identity);
            bomb.StartBombCount(this);
            bombInfo.direction = -bombInfo.direction;
        }
    }
}