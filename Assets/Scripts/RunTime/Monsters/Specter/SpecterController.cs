using Cysharp.Threading.Tasks;
using Game.Monsters.BlackKnight;
using UnityEngine;

namespace Game.Monsters.Specter
{
    public class SpecterController : MonsterControllerBase<SpecterController>,IRangeAttack
    {
        public GameObject rangeAttackObj { get; set; }
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
            SetHitJudgementObject();
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
        public override async void DestroyAll()
        {
            var deathState = DeathState as DeathState;
            await UniTask.WaitUntil(deathState.getDeathMoverStatus);
            base.DestroyAll();
        }
    }
}