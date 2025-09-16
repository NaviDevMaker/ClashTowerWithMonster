using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class WerewolfController : MonsterControllerBase<WerewolfController>, INonTarget
    {
        public ShapeShiftState ShapeShiftState { get; private set; }
        public bool IsInvincible { get; set; } = false;
        public float shapeShiftDuration => 0.5f;

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
        protected override void Update()
        {

            if (!isSummonedInDeckChooseScene)
            {
                if (!IsInvincible)
                {
                    HPBarProcess();
                    Debug.Log($"{statusCondition.Freeze.isActive},{statusCondition.Freeze.isEffectedCount}");
                    this.CheckFreeze_Unit(animator);
                    this.CheckAbsorption();
                    if (isSummoned)
                    {
                        currentState?.OnUpdate();
                    }
                    Debug.Log(currentState);
                }
                if (isDead && currentState != DeathState)
                {
                    ChangeToDeathState();
                }
            }
            //これデッキ選択シーンの時に見本用のモンスターをその都度削除するからそのため
            else
            {
                if (isDead && currentState != DeathState)
                {
                    ChangeToDeathState();
                }
            }
        }
        public override void Damage(int damage)
        {
            base.Damage(damage);
            if (isDead) return;
            if (currentHP <= maxHP / 2 && currentState != ShapeShiftState && !isShapeShifted)
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

        public void ReflectEachHP(int currentHP) => this.currentHP = currentHP;
    }
}