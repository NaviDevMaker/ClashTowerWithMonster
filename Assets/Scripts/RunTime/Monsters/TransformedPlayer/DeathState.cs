using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Monsters.TransformedPlayer
{
    public class DeathState : DeathStateBase<TransformedPlayer>
    {
        public DeathState(TransformedPlayer controller) : base(controller) { }

        public override async void OnEnter()
        {
            var animPar = controller.MonsterAnimPar;
            controller.animator.SetBool(animPar.Attack_Hash, false);
            controller.animator.SetBool(animPar.Chase_Hash, false);
            controller.animator.Play("Idle");
            await WaitFadeOut();
            if (controller != null) UnityEngine.Object.Destroy(controller.gameObject);
        }
        public override void OnUpdate() { }
        public override void OnExit() { }

        async UniTask WaitFadeOut()
        {
            var duration = 2.0f;
            var tasks = controller.meshMaterials.SelectMany(materials => materials.Select(material =>
            {
                FadeProcessHelper.ChangeToTranparent(material);
                return FadeProcessHelper.FadeOutColor(duration, material, controller.GetCancellationTokenOnDestroy());
            })).ToArray();
            await UniTask.WhenAll(tasks);
        }
    }
}