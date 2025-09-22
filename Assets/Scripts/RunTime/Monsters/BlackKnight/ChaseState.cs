using UnityEngine;

namespace Game.Monsters.BlackKnight
{
    public class ChaseState : ChaseStateBase<BlackKnightController>
    {
        public ChaseState(BlackKnightController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            Debug.Log($"�G��{targetEnemy},�^���[��{targetTower}");
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
    }

}