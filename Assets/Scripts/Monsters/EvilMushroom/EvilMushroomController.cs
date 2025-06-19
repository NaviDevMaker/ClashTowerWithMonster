using UnityEngine;

namespace Game.Monsters.EvilMushroom
{
    public class EvilMushroomController : MonsterControllerBase<EvilMushroomController>
    {

        protected override void Awake()
        {
            base.Awake();
            isSummoned = true;//テスト用だから消して
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }

        public override void Initialize(int owner = -1)
        {
            //Please select your monster movetype.
            moveType = MoveType.Walk;
            base.Initialize(owner);
            //I recommend to delete comment out after you create state class at Auto State Creater
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }

    }

}