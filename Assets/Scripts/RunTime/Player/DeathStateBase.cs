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
        public override void OnEnter()
        {
            controller.OnDeathPlayer(true);
            clipLength = controller.GetAnimClipLength();
            //StartDeathAction().Forget();
            controller.ExecuteDeathAction_Player(clipLength, stateAnimSpeed).Forget();
        }

        public override void OnExit()
        {
           
        }

        public override void OnUpdate()
        {
            
        }
    }
}


