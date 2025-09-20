using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine.Events;
using System;
namespace Game.Monsters.TransformedPlayer
{
    public class IdleState : IdleStateBase<TransformedPlayer>
    {
        public IdleState(TransformedPlayer controller) : base(controller) { }


        public override void OnEnter()
        {
            OnEnterProcess().Forget();
        }
        public override void OnUpdate()
        {
           base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask OnEnterProcess()
        {
            nextState = controller.ChaseState;
            var collider = controller.GetComponent<Collider>();
            collider.enabled = false;
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    var color = mat.color;
                    color.a = 0f;
                    mat.color = color;
                    Debug.Log(mat.color);
                }
            }
            await controller.WaitFIAllMesh(controller.shapeShiftDuration);
            if (controller == null) return;
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    FadeProcessHelper.ChangeToOpaque(mat);
                }
            }
            isEndSummon = true;
            controller.IsInvincible = false;
            collider.enabled = true;
        }
    }
}