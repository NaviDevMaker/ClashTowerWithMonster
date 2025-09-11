using Cysharp.Threading.Tasks;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Game.Monsters.SpiderEgg
{
    public class SwayState : StateMachineBase<SpiderEggController>
    {
        public SwayState(SpiderEggController controller) : base(controller) { }

        float swayDuration = 10.0f;
        float speedupDuration = 2.0f;
        float elapsedTime = 0f;
        bool isEndWait = false;
        public override void OnEnter()
        {
            controller.animator.enabled = true;
            controller.animator.SetBool(controller.sway_Hash, true);
            nextState = controller.SpawnSpiderState;
            WaitDuration();
        }
        public override void OnExit()
        {
            controller.animator.SetBool(controller.sway_Hash,false);
            controller.animator.speed = 1.0f;
        }

        public override void OnUpdate()
        {
            var pos = controller.transform.position;
            pos.y = Terrain.activeTerrain.SampleHeight(pos);
            controller.transform.position = pos;
            if (isEndWait) controller.ChangeState(nextState);
        }
        async void WaitDuration()
        {
            var startTime = Time.time;
            try
            {
                while(Time.time - startTime < swayDuration && !controller.isDead)
                {
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime > speedupDuration)
                    {
                        elapsedTime = 0f;
                        controller.animator.speed++;
                    }
                    await UniTask.Yield(cancellationToken: controller.GetCancellationTokenOnDestroy());
                }
                if(!controller.isDead) isEndWait = true;

            }
            catch (OperationCanceledException) { }
        }
    }

}

