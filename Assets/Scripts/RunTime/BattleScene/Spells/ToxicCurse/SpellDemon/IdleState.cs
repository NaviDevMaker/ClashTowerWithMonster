using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
namespace Game.Monsters.SpellDemon
{
    public class IdleState : IdleStateBase<SpellDemonController>
    {
        public IdleState(SpellDemonController controller) : base(controller) { }


        public override void OnEnter()
        {
            OnEnterProcess().Forget();
        }
        public override void OnUpdate()
        {
           base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask OnEnterProcess()
        {
            AppearranceProcess();
            var waitTime = controller.MonsterStatus.SummonWaitTime;
            var task = UniTask.Delay(TimeSpan.FromSeconds(waitTime));//waitTime
            var task2 = UniTask.WaitUntil(() => controller.EndSetProcess);
            await UniTask.WhenAll(task, task2);
            nextState = controller.AttackState;
            isEndSummon = true;
        }

        void AppearranceProcess()
        {
            var duration = 0.5f;
            EffectManager.Instance.statusConditionEffect.toxicSmokeEffect.ToxicEffectSet(controller,duration);
            var unitMaterials = controller.meshMaterials;
            foreach (var materials in unitMaterials)
            {
                foreach (var material in materials)
                {
                    FadeProcessHelper.FadeInColor(duration, material,controller.GetCancellationTokenOnDestroy()).Forget();
                }
            }
        }
        protected override void AllResetBoolProparty() => controller.animator.SetBool(controller.MonsterAnimPar.Attack, false);

    }

}