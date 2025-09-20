using Cysharp.Threading.Tasks;
using Game.Monsters.WormMonster;
using System;
using UnityEngine;
using System.Threading;

namespace Game.Monsters.WormMonster
{
    public class BurrowChaseState : ChaseStateBase<WormMonsterController>
    {
        public BurrowChaseState(WormMonsterController controller) : base(controller) 
        {
            moveEffect = controller.transform.GetChild(2).GetComponent<ParticleSystem>();
        }
        readonly int diving_Hash = Animator.StringToHash("isGroundDiving");
        ParticleSystem moveEffect;
        public override async void OnEnter()
        {           
            try
            {
                await WaitUntilGroundDive();
            }
            catch (OperationCanceledException) { return; }
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            controller.IsInvincible = false;
            controller.statusCondition.NonTarget.isActive = false;
            moveEffect.Stop();
            base.OnExit();
        }
        async UniTask WaitUntilGroundDive()
        {
            controller.wormEffect.GenerateWormEffect();
            controller.animator.SetBool(diving_Hash, true);
            try
            {
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName("GroundDive")
                                              ,cancellationToken: controller.GetCancellationTokenOnDestroy());
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f
                                              , cancellationToken: controller.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) {throw;}
            finally
            {
                controller.statusCondition.NonTarget.isActive = true;
                controller.IsInvincible = true;
                controller.animator.SetBool(diving_Hash, false);
                moveEffect.Play();
            }
        }
    }
}


