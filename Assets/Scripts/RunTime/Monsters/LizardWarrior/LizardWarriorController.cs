using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.LizardWarrior
{
    public class LizardWarriorController : MonsterControllerBase<LizardWarriorController>,IRangeAttack
    {
        public GameObject rangeAttackObj { get; set; }

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
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
        public void SetHitJudgementObject() { }
        public override async void DestroyAll()
        {
            if (hPBar != null) Destroy(hPBar.gameObject);
            DeathState death = DeathState as DeathState;
            await UniTask.WaitUntil(() =>
            {
                if (death != null) return death.FireLingActionEnd;
                else return false;
            });
            Debug.Log("リザードウォーリアーが破壊されます");
            if (this != null && this.gameObject != null) Destroy(this.gameObject);
        }
    }
}