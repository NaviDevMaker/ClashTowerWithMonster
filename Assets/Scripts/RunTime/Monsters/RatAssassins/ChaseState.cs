using UnityEngine;

namespace Game.Monsters.RatAssassin
{
    public class ChaseState : ChaseStateBase<RatAssassinController>
    {
        public ChaseState(RatAssassinController controller) : base(controller) { }

        ParticleSystem runParticle;
        public override void OnEnter()
        {
            runParticle = controller.GetComponentInChildren<ParticleSystem>();
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            if(targetEnemy != null)
            {
                if(!runParticle.isPlaying) runParticle.Play();
                currentMoveSpeed = controller.MonsterStatus.MoveSpeed * 3f;
                baseAnimSpeed = 2.0f;
            }
            else
            {
                if(!runParticle.isStopped) runParticle.Stop();
                currentMoveSpeed = controller.MonsterStatus.MoveSpeed;
                baseAnimSpeed = 1.0f;
            }
            var isFreezed = controller.statusCondition.Freeze.isActive;
            if(!isFreezed) controller.animator.speed = baseAnimSpeed;
            base.OnUpdate();
        }
        public override void OnExit()
        {
            if (runParticle.isPlaying) runParticle.Stop();
            base.OnExit();
        }
    }
}