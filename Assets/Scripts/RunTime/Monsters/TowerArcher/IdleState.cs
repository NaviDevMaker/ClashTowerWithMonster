using UnityEngine;


namespace Game.Monsters.Archer
{
    public class IdleState : StateMachineBase<ArcherController>
    {
       public IdleState(ArcherController controller):base(controller) { }
        public override void OnEnter()
        {
            nextState = controller._BowShotState;
        }

        public override void OnUpdate()
        {
            Debug.Log(controller.target);
            if (controller.target != null) controller.ChangeState(nextState);
        }

        public override void OnExit() { }
    }
}


