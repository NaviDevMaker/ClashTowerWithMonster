
using Cysharp.Threading.Tasks;

namespace Game.Monsters
{
    public class DeathStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public DeathStateBase(T controler) : base(controler) { }
        float stateAnimSpeed = 0f;
        public override void OnEnter()
        {
            controller.animator.speed = 1.0f;
            stateAnimSpeed = controller.MonsterStatus.AnimaSpeedInfo.DeathStateAnimSpeed;
            clipLength = controller.GetAnimClipLength();
            //DeathMove().Forget();
            controller.ExecuteDeathAction_Monster(clipLength,stateAnimSpeed).Forget();
        }

        public override void OnUpdate() { }
        public override void OnExit()
        {

        }
    }
}

