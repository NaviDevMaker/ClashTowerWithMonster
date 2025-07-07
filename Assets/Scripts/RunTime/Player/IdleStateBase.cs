using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Players
{
    public class IdleStateBase<T> : StateMachineBase<T> where T : PlayerControllerBase<T>
    {
       public IdleStateBase(T controller):base(controller) { }
        public override void OnEnter()
        {
            nextState = controller.MoveState;
            OnEnterProcess();
        }

        public override void OnUpdate()
        {
           if(!controller.isUsingSkill)
           {
                if (InputManager.IsClickedMoveAndAutoAttack())
                {
                    Debug.Log("‰Ÿ‚³‚ê‚Ü‚µ‚½");
                    controller.ChangeState(nextState);
                    controller.MoveState.isPressedA = true;
                    return;
                }
           }
          
           if (InputManager.IsCllikedMoveButton()) controller.ChangeState(nextState);
        }
        public override void OnExit()
        {
            Debug.Log($"{controller.name}‚ÌisIdle‚ªfalse‚É‚È‚è‚Ü‚µ‚½");
            controller.animator.SetBool(controller.AnimatorPar.Idle, false);
        }
        protected virtual void OnEnterProcess() => AllResetBoolProparty();

        void AllResetBoolProparty()
        {
            controller.animator.SetBool(controller.AnimatorPar.Idle, true);
            Debug.Log($"{controller.AnimatorPar.Move},{controller.AnimatorPar.Attack}");
            if(controller.animator.GetBool(controller.AnimatorPar.Move)) controller.animator.SetBool(controller.AnimatorPar.Move, false);
        }
    }
    
}


