using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.EvilMage
{
    public class AttackState : AttackStateBase<EvilMageController>
        ,ILongDistanceAction<EvilMageController>
    {
        public AttackState(EvilMageController controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<EvilMageController >(controller, this, clipLength,10,
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
        protected override async UniTask Attack_Long(LongAttackArguments<EvilMageController> longAttackArguments)
        {
            var arguments = new LongAttackArguments<EvilMageController>
            {
                getNextMover = GetNextMover,
                moveAction = NextMoverAction
            };
            await base.Attack_Long(arguments);
        }
        public LongDistanceAttack<EvilMageController> GetNextMover()
        {
            foreach (var mover in controller.movers)
            {
                if(mover is MageSpellMover mageMover)
                {
                    if (!mageMover.gameObject.activeInHierarchy && !mageMover.IsProcessingTask)
                    {
                        return mover;
                    }
                }            
            }
            return null;
        }
        public void NextMoverAction(LongDistanceAttack<EvilMageController> nextMover)
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