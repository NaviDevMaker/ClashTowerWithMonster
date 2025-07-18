using UnityEngine;


public class BuffTime
{
    public float buffInverval { get; set; }
    public float time { get; set; }
}
namespace Game.Monsters.AttackerBoy
{
    public class AttackerBoyController : MonsterControllerBase<AttackerBoyController>
    {
        public BuffState BuffState { get; private set; }
        BuffTime buffTime;
        protected override void Awake()
        {
            base.Awake();
            isSummoned = true;//テスト用だから消して
        }

        protected override void Update()
        {
            base.Update();
            CheckBuffInverval();
            BuffState.CheckIsUnitInRange();
            Debug.Log(currentState);
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }
        public override void Initialize(int owner = -1)
        {
            //Please select your monster movetype.
            moveType = MoveType.Walk;
            base.Initialize(owner);
            //I recommend to delete comment out after you create state class at Auto State Creater
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            BuffState = new BuffState(this);
            DeathState = new DeathState(this);

            buffTime = new BuffTime { buffInverval = 3.0f, time = 0f };
            BuffState.ResetTime = (() => buffTime.time = 0f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position,8f);
        }
        void CheckBuffInverval()
        {
            if (currentState == BuffState || isDead) return;
            var time = buffTime.time += Time.deltaTime;
            var inverval = buffTime.buffInverval;

            //Debug.Log(time);
            if(time >= inverval && !BuffState.wasBuffedFailed && !isDead)
            {
                ChangeState(BuffState);
            }

            if(BuffState.wasBuffedFailed && !isDead)
            {
                if(BuffState.unitIsRange) ChangeState(BuffState);
            }

        }
    }

}