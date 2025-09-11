using UnityEngine;

namespace Game.Monsters.Skeleton
{
    public class SkeletonController : MonsterControllerBase<SkeletonController>
    {
        [System.Serializable]
        public class GuirdInfo
        {
            [SerializeField] MeshRenderer shieldMesh;

            public readonly float guirdDuration = 8.0f;
            public bool isGuirding { get; set; } = false;
            public bool isInvokedGuird { get; set; } = false;
            public MeshRenderer ShieldMesh  => shieldMesh;
        }

        public GuirdInfo guirdInfo;
        public override Renderer BodyMesh
        { 
            get
            {
                if (guirdInfo.isGuirding) return guirdInfo.ShieldMesh;
                else return base.BodyMesh;
            }
        }

        GuirdState GuirdState;

        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            GuirdState = new GuirdState(this);
        }
        public override void Damage(int damage)
        {
            if(!guirdInfo.isGuirding && !guirdInfo.isInvokedGuird && currentHP - damage <= 0)
            {
                guirdInfo.isInvokedGuird = true;
                ChangeState(GuirdState);
            }
            if (guirdInfo.isGuirding) return;
            base.Damage(damage);
        }
    }
}