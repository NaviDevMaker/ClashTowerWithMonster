using UnityEngine;

namespace Game.Monsters.TransformedPlayer
{
    public class ChaseState : ChaseStateBase<TransformedPlayer>
    {
        public ChaseState(TransformedPlayer controller) : base(controller) { }

        public override void OnEnter()
        {
            controller.originalEntity.animator.SetBool(controller.MonsterAnimPar.Chase_Hash, true);
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            controller.originalEntity.animator.SetBool(controller.MonsterAnimPar.Chase_Hash,false);
            base.OnExit();
        }
    }

}