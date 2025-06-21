using UnityEngine;

namespace Game.Monsters.SpeederBoy
{
    public class SpeederBoyController : MonsterControllerBase<SpeederBoyController>
    {
        public BuffState BuffState { get; private set; }
        BuffTime buffTime;
        protected override void Awake()
        {
            base.Awake();
            isSummoned = true;//テスト用だから消して
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

            buffTime = new BuffTime { buffInverval = 3.0f, time = 0f };
            BuffState.ResetTime = (() => buffTime.time = 0f);
        }

        void CheckBuffInverval()
        {
            if (currentState == BuffState) return;
            var time = buffTime.time += Time.deltaTime;
            var inverval = buffTime.buffInverval;

            //Debug.Log(time);
            if (time >= inverval && !BuffState.wasBuffedFailed)
            {
                ChangeState(BuffState);
            }

            if (BuffState.wasBuffedFailed)
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