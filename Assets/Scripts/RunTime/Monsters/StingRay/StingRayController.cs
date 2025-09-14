using UnityEngine;

namespace Game.Monsters.StingRay
{
    public class StingRayController : MonsterControllerBase<StingRayController>
        ,IRangeAttack,IRepeatAttack
    {
        public GameObject rangeAttackObj { get; set; }
        public int repeatCount => 10;

        protected override void Awake()
        {
            SetHitJudgementObject();
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }

        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Fly;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
        public void SetHitJudgementObject()
        {
            var data = _FlyProjectileAttackMonsterStatus;
            if (data == null) return;
            var weponName = data.ProjectileHitJudgeObj.name;
            rangeAttackObj = this.gameObject.GetObject(weponName);
            Debug.Log(rangeAttackObj.name);
        }
    }
}