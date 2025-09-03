using UnityEngine;

namespace Game.Monsters.SpeederBoy
{
    public class SpeederBoyController : MonsterControllerBase<SpeederBoyController>,ISpecialIntervalActionInfo
    {
        public BuffState BuffState { get; private set; }

        public float actionInverval => 3.0f;

        public float elapsedTime { get;set; } = 0f;

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
        }
        protected override void Update()
        {
            base.Update();
            if (isSummonedInDeckChooseScene) return;
            CheckBuffInverval();
            BuffState.CheckIsUnitInRange();
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

            BuffState.ResetTime = (() => elapsedTime = 0f);
        }

        void CheckBuffInverval()
        {
            if (currentState == BuffState || isDead) return;
            var time = elapsedTime += Time.deltaTime;
            //Debug.Log(elapsedTime);
            if (time >= actionInverval && !BuffState.wasBuffedFailed && !isDead)
            {
                ChangeState(BuffState);
            }

            if (BuffState.wasBuffedFailed && !isDead)
            {
                if (BuffState.unitIsRange) ChangeState(BuffState);
            }

        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 8f);
        }
    }

}