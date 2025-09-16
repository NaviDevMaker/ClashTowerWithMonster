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
                origin.gameObject.SetActive(true);
                origin.isDead = true;
            }
            if (controller != null) UnityEngine.Object.Destroy(controller.gameObject);
        }
        public override void OnUpdate() { }
        public override void OnExit() { }

        async UniTask ShapeShiftToOriginal(WerewolfController origin)
        {
            var animPar = controller.MonsterAnimPar;
            controller.animator.SetBool(animPar.Attack_Hash, false);
            controller.animator.SetBool(animPar.Chase_Hash, false);
            controller.animator.Play("Idle");
            controller.IsInvincible = true;
            //controller.transform.SetParent(origin.transform);
            var shapeShiftDuration = controller.shapeShiftDuration;
            var seq = controller.GetTransformSequence(shapeShiftDuration);
            var seqTask = seq.ToUniTask(cancellationToken:controller.GetCancellationTokenOnDestroy());
            await UniTask.WhenAll(controller.WaitFOAllMesh(shapeShiftDuration), seqTask);
            controller.ShapeEffectAction();
            origin.transform.position = controller.transform.position;
            origin.ShapeShiftState.EndShapeShiftAction();
            origin.ReflectEachHP(controller.currentHP);
            origin.transform.SetParent(null);
        }
    }
}