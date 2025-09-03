using Game.Monsters.BlackKnight;
using UnityEngine;

namespace Game.Monsters.Golem
{
    public class GolemController : MonsterControllerBase<GolemController>,IRangeAttack
    {
        public GameObject rangeAttackObj { get;set; }

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
            Debug.Log(rangeAttackObj.name);
        }
    }
}