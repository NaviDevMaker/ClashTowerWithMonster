using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.MonsterPlant
{
    public class AttackState : AttackStateBase<MonsterPlantController>
    {
        public AttackState(MonsterPlantController controller) : base(controller) { }
        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<MonsterPlantController >(controller, this, clipLength,13,
                controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        {
            var arguments = new SimpleAttackArguments
            { 
                getTargets = attackArguments.getTargets,
                specialEffectAttack = (target) => HealHp(),
            };
            await base.Attack_Generic(arguments);
        }
        void HealHp()
        {
            var amount = Mathf.RoundToInt(controller.MonsterStatus.AttackAmount / 2f);
            controller.Heal(amount);
            EffectManager.Instance.healEffect.GenerateUnitHealEffect(controller);
        }
    }
}