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
        public override void OnEnter()
        {
            
            clipLength = controller.GetAnimClipLength();
            controller.OnDestoryedTower?.Invoke(clipLength, stateAnimSpeed);
            controller.ExecuteDeathAction_Archer(clipLength, stateAnimSpeed).Forget();

        }
        public override void OnUpdate() { }
        public override void OnExit() { }      
    }
}

