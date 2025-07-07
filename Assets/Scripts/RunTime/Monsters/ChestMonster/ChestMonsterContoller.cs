using Game.Monsters;
using UnityEngine;


namespace Game.Monsters.ChestMonster
{
    public class ChestMonsterContoller : MonsterControllerBase<ChestMonsterContoller>
    {
        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
        }

        public override void Initialize(int owner)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
    }

}

