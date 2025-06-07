using Cysharp.Threading.Tasks;
using Game.Monsters.Archer;
using System.Threading;
using System;
using UnityEngine;

namespace Game.Monsters.Archer
{
    public class DeathState : StateMachineBase<ArcherController>
    {
        public DeathState(ArcherController controller) : base(controller) { }
        float stateAnimSpeed = 1.0f;
        DeathMoveExecuter deathMoveExecuter;
        public override void OnEnter()
        {
            deathMoveExecuter = new DeathMoveExecuter();
            clipLength = controller.GetAnimClipLength();
            //DeathMove().Forget();
            deathMoveExecuter.ExecuteDeathAction_Archer(controller, clipLength, stateAnimSpeed).Forget();

        }

        public override void OnUpdate() { }
        public override void OnExit() { }
       
    }

}

