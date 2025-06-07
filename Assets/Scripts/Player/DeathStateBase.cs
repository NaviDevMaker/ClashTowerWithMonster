using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


namespace Game.Players
{
    public class DeathStateBase<T> : StateMachineBase<T> where T : PlayerControllerBase<T>
    {
        public DeathStateBase(T controller) : base(controller) { }
        protected float stateAnimSpeed = 0f;
        DeathMoveExecuter deathMoveExecuter;
        public override void OnEnter()
        {
            controller.OnDeathPlayer(true);
            deathMoveExecuter = new DeathMoveExecuter();
            clipLength = controller.GetAnimClipLength();
            //StartDeathAction().Forget();
            deathMoveExecuter.ExecuteDeathAction_Player<T>(controller, clipLength, stateAnimSpeed).Forget();

        }

        public override void OnExit()
        {
           
        }

        public override void OnUpdate()
        {
            
        }
    }
}


