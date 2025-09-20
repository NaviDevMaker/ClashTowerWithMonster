using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class AttackState : AttackStateBase<WerewolfController>
    {
        public AttackState(WerewolfController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<WerewolfController >(controller,this,clipLength,1,
                controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            isAttacking = false;
            base.OnExit();
        }

        protected override async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        {
            var argument = new SimpleAttackArguments
            {
                getTargets = attackArguments.getTargets,
                repeatCount = controller.ContinuousAttackMonsterStatus._ContinuousAttackInfo.ContinuousCount
            };

            await base.Attack_Generic(argument);
        }
    }

}