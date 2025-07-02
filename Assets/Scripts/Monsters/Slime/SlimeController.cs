using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.Slime
{
    public class SlimeController : MonsterControllerBase<SlimeController>
    {
        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//�e�X�g�p�����������
        }
        //public int ID;//�e�X�g�p����������Ă�
        protected override void Start()
        {
            Debug.Log("��������������������������������������������");
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
    }
}

