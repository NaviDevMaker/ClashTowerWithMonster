using UnityEngine;

namespace Game.Monsters.GuirdSlime
{
    public class GuirdSlimeController : MonsterControllerBase<GuirdSlimeController>
    {

        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            base.Start();
        }

        public override void Initialize(int owner = -1)
        {
            Debug.Log("dsajnskajdsfksj");
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }

    }

}


