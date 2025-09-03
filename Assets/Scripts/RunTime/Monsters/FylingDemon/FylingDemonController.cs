using UnityEngine;

namespace Game.Monsters.FylingDemon
{
    public class FylingDemonController : MonsterControllerBase<FylingDemonController>,ISpecialIntervalActionInfo
    {
        public CallHelpState CallHelpState { get; private set;}

        public GameObject demonObj { get; private set; }
        public float actionInverval => 10f;
        public float elapsedTime { get; set; } = 0f;

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
            if (isSummonedInDeckChooseScene) return;
            CheckCallHelpInterval();
        }
        public async override void Initialize(int owner = -1)
        {
            moveType = MoveType.Fly;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            CallHelpState = new CallHelpState(this);
            demonObj = await SetFieldFromAssets.SetField<GameObject>("Monsters/FlyingDemon");
        }
        void CheckCallHelpInterval()
        {
            if (currentState == CallHelpState || isDead) return;
            var time = elapsedTime += Time.deltaTime;
            if(time >= actionInverval)
            {
                Debug.Log("仲間を呼びます");
                ChangeState(CallHelpState);
            }
        }
        public void SetSummonParticle(Vector3 particlePos)
        {
            StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(particlePos, CardType.Monster));
        }
    }
}