using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.Slime
{
    public class SlimeController : MonsterControllerBase<SlimeController>
    {
        //public int ID;//�e�X�g�p����������Ă�
        protected override void Start()
        {
            base.Start();
        }

        public override void Initialize(int owner = -1)
        {
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }

    }

}

