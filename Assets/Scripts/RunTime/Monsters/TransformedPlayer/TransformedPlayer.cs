using Game.Monsters.Werewolf;
using UnityEngine;

public interface INonTarget
{
    UnitBase baseEntity { get; set; }
    float nonTargetInterval { get; set; }
    float elapsedTime { get; }
}

namespace Game.Monsters.TransformedPlayer
{
    public class TransformedPlayer : MonsterControllerBase<TransformedPlayer>, INonTarget
    {
        public UnitBase baseEntity { get; set; }
        public float nonTargetInterval { get; set; } = 30f;
        public float elapsedTime { get; private set;}

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
            if(IdleState.isEndSummon) CheckNonTarget();
            base.Update();
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            statusCondition.NonTarget.isActive = true;
        }
        void CheckNonTarget()
        {
            elapsedTime += Time.deltaTime;
            if(nonTargetInterval <= elapsedTime && !isDead) isDead = true;
        }
    }
}