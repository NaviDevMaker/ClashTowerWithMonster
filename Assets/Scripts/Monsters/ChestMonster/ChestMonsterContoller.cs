using Game.Monsters;
using UnityEngine;


namespace Game.Monsters.ChestMonster
{
    public class ChestMonsterContoller : MonsterControllerBase<ChestMonsterContoller>
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
        }

        public override void Initialize(int owner)
        {
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
    }

}

