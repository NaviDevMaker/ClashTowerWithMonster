using UnityEngine;


namespace Game.Players.Sword
{
    public class SwordPlayerController : PlayerControllerBase<SwordPlayerController>
    {
  
        public override void Initialize(int owner)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            MoveState = new MoveState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);  
            CheckEnemyState = new CheckEnemyState(this);   
        }
    }

}

