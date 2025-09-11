using UnityEngine;

namespace Game.Monsters.BlackKnight
{
    public class BlackKnightController : MonsterControllerBase<BlackKnightController>,IRangeAttack
    {
        public GameObject rangeAttackObj { get; set;}
        public ShockWaveEffecter waveEffecter { get; private set;}
        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
            SetHitJudgementObject();
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
            waveEffecter = rangeAttackObj.AddComponent<ShockWaveEffecter>();
            waveEffecter.Initialize(this);
            Debug.Log(rangeAttackObj.name);
        }
    }
}