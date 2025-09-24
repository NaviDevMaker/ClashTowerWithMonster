using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.KingDragon
{
    public class DeathState : StateMachineBase<KingDragonController>
    {
        public DeathState(KingDragonController controller) : base(controller) { }
        public override void OnEnter()
        {
            controller.animator.SetTrigger(controller.KingDragonAnimPar.Death_Hash);
            clipLength = controller.animator.
                         GetAnimationClip(controller.KingDragonAnimPar.deathAnimClipName).length / 0.5f;
            controller.ExecuteDeathAction_Tower(clipLength).Forget();
        }
        public override void OnExit()
        {
        }
        public override void OnUpdate()
        {
        }
    }
}

