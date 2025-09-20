using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Monsters.WormMonster
{
    public class AttackState : AttackStateBase<WormMonsterController>
                              ,AttackStateBase<WormMonsterController>.ILongDistanceAction
                              ,IEffectSetter
    {
        public AttackState(WormMonsterController controller) : base(controller) => SetEffect();

        readonly int break_Hash = Animator.StringToHash("isGroundBreaking");
        bool isGroundBreak = false;
        ParticleSystem mouthEffect;
        public override async void OnEnter()
        {
            try
            {
                await WaitUntilGroundBreak();
            }
            catch (OperationCanceledException) { return;}
            SetUp();
            if (!isSettedEventClip) ChangeClipForAnimationEvent();//
            nextState = controller.BurrowChaseState;
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<WormMonsterController>
                    (controller, this, clipLength,10,controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            if (!isGroundBreak) return;
            base.OnUpdate();
        }
        public override void OnExit()
        {
            isGroundBreak = false;
            base.OnExit();
        }

        protected override async UniTask Attack_Long(LongAttackArguments longAttackArguments)
        {
            var arguments = new LongAttackArguments
            { 
                getNextMover = GetNextMover,
                moveAction = NextMoverAction,
                attackEffectAction = () => mouthEffect.Play()
            };
            await base.Attack_Long(arguments);
        }
        async UniTask WaitUntilGroundBreak()
        {
            cts = new CancellationTokenSource();
            controller.wormEffect.GenerateWormEffect();
            controller.animator.SetBool(break_Hash, true);
            try
            {
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName("GroundBreak")
                                              ,cancellationToken:cts.Token);
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f
                                              ,cancellationToken:cts.Token);
            }
            catch (OperationCanceledException) { throw; }
            finally
            {
                isGroundBreak = true;
                controller.animator.SetBool(break_Hash,false);
            }
        }

        public LongDistanceAttack<WormMonsterController> GetNextMover()
        {
            foreach (var mover in controller.movers)
            {
                if (mover is NeurotoxinMover neurontoxicMover)
                {
                    if (!neurontoxicMover.gameObject.activeInHierarchy)
                    {
                        return mover;
                    }
                }
            }
            return null;
        }

        public void NextMoverAction(LongDistanceAttack<WormMonsterController> nextMover)
        {
            if (nextMover != null)
            {
                nextMover.target = this.target;
                nextMover.gameObject.SetActive(true);
                nextMover.Move();
                Debug.Log("‘Å‚½‚ê‚Ü‚µ‚½");
            }
        }

        public async void SetEffect()
        {
            var effectObjPrefab = await SetFieldFromAssets.SetField<GameObject>("Monsters/WormMonsterMouthEffect");
            if (effectObjPrefab == null) return;
            var pos = controller.startTra.transform.position;
            var rot = effectObjPrefab.transform.rotation;
            var effectObj = UnityEngine.Object.Instantiate(effectObjPrefab,pos,rot);
            effectObj.transform.SetParent(controller.startTra);
            mouthEffect = effectObj.GetComponent<ParticleSystem>();
        }
    }
}