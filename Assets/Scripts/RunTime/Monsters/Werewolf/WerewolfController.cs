using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class WerewolfController : MonsterControllerBase<WerewolfController>
    {
        public ShapeShiftState ShapeShiftState { get;private set; }
        bool isShapeShifted = false;
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

        public override void Damage(int damage)
        {
            var isNontarget = statusCondition.NonTarget.isActive;
            if (isNontarget) return;
            base.Damage(damage);
            if(currentHP <= maxHP / 2 && currentState != ShapeShiftState && !isShapeShifted)
            {
                isShapeShifted = true;
                ChangeState(ShapeShiftState);
            }
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            ShapeShiftState = new ShapeShiftState(this);
        }
    }
}