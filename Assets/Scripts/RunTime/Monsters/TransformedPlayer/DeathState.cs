using Cysharp.Threading.Tasks;
using Game.Monsters.Werewolf;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Game.Monsters.TransformedPlayer
{
    public class DeathState : DeathStateBase<TransformedPlayer>
    {
        public DeathState(TransformedPlayer controller) : base(controller) { }

        public override async void OnEnter()
        {
            controller.DisableHpBar();
            var origin = controller.originalEntity;
            //Ç±ÇÍÇÕÇ¬Ç‹ÇËéÙï∂Ç‚îÕàÕçUåÇÇ≈éÄÇÒÇæéû
            if (controller.currentHP > 0)
            {
                await ShapeShiftToOriginal(origin);
            }
            else
            {
                Debug.Log("éÙï∂Ç≈éÄÇÒÇæÇΩÇﬂÅAoriginÇÕéÄÇ…Ç‹Ç∑");
                origin.isDead = true;
            }
            if (controller != null) UnityEngine.Object.Destroy(controller.gameObject);
        }
        public override void OnUpdate() { }
        public override void OnExit() { }

        async UniTask ShapeShiftToOriginal(WerewolfController origin)
        {
            origin.transform.position = controller.transform.position;
            controller.transform.SetParent(origin.transform);
            origin.ShapeShiftState.EndShapeShiftAction();
            origin.ReflectEachHP(controller.currentHP);
            controller.IsInvincible = true;
            var animPar = controller.MonsterAnimPar;
            controller.animator.SetBool(animPar.Attack_Hash, false);
            controller.animator.SetBool(animPar.Chase_Hash, false);
            controller.animator.Play("Idle");
            var duration = 2.0f;
            await controller.WaitFOAllMesh(duration);
        }
    }
}