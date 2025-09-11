using Cysharp.Threading.Tasks;
using Game.Monsters.EvilMage;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monsters.Salamander
{
    public class AttackState : AttackStateBase<SalamanderController>
        ,AttackStateBase<SalamanderController>.ILongDistanceAction,IEffectSetter
    {
        public AttackState(SalamanderController controller) : base(controller)
        {
            SetEffect();
        }
        ParticleSystem mouthFireEffect;
        Queue<ParticleSystem> particles = new Queue<ParticleSystem>();
        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<SalamanderController >(controller, this, clipLength,18,
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
        protected override async UniTask Attack_Long(LongAttackArguments longAttackArguments)
        {
            var arguments = new LongAttackArguments
            { 
                attackEffectAction = PlayMouthFireEffect,
                attackEndAction = DestroyParticle,
                getNextMover = GetNextMover,
                moveAction = NextMoverAction
            };
            await base.Attack_Long(arguments);
        }
        public async void SetEffect()
        {
            var obj = await SetFieldFromAssets.SetField<GameObject>("Effects/SalamanderMouthEffect");
            if (obj != null) mouthFireEffect = obj.GetComponent<ParticleSystem>();
        }
        void PlayMouthFireEffect()
        {
            if (mouthFireEffect == null) return;
            var startTra = controller.startTra;
            var pos = startTra.position;
            var particle = UnityEngine.Object.Instantiate(mouthFireEffect,pos,mouthFireEffect.transform.rotation);
            var main = particle.main;
            main.loop = true;
            particle.Play();
            particle.transform.SetParent(startTra);
            particles.Enqueue(particle);
        }
        async void DestroyParticle()
        {
            var particle = particles.Dequeue();
            var main = particle.main;
            main.loop = false;
            main.simulationSpeed *= 3f;
            await RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            if (particle != null) UnityEngine.Object.Destroy(particle.gameObject);
        }

        public LongDistanceAttack<SalamanderController> GetNextMover()
        {
            foreach (var mover in controller.movers)
            {
                if (mover is FireMover fireMover)
                {
                    if (!fireMover.gameObject.activeInHierarchy && !fireMover.IsProcessingTask)
                    {
                        return mover;
                    }
                }
            }
            return null;
        }
       
        public void NextMoverAction(LongDistanceAttack<SalamanderController> nextMover)
        {
            if (nextMover != null)
            {
                nextMover.target = this.target;
                nextMover.gameObject.SetActive(true);
                nextMover.Move();
                Debug.Log("‘Å‚½‚ê‚Ü‚µ‚½");
            }
        }
    }
}