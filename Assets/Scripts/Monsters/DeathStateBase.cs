using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEditor.Search;
using System.Collections.Generic;

namespace Game.Monsters
{
    public class DeathStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public DeathStateBase(T controler) : base(controler) { }
        protected float stateAnimSpeed = 0f;
        DeathMoveExecuter deathMoveExecuter;
        public override void OnEnter()
        {
            deathMoveExecuter = new DeathMoveExecuter();
            clipLength = controller.GetAnimClipLength();
            //DeathMove().Forget();
            deathMoveExecuter.ExecuteDeathAction_Monster<T>(controller, clipLength,stateAnimSpeed).Forget();
        }

        public override void OnUpdate() { }
        public override void OnExit()
        {

        }
    }
}

