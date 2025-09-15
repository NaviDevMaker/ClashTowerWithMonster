using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class ShapeShiftState : StateMachineBase<WerewolfController>
    {
        public ShapeShiftState(WerewolfController controller) : base(controller)
        {
            SetTransformedPlayerPrefab();
        }
        TransformedPlayer.TransformedPlayer transformedPlayerPrefab;
        float shapeShiftDuration;
        bool isEndShapeAction = false;
        public override void OnEnter()
        {
            try
            {
                controller.ChaseState.cts?.Cancel();
            }
            catch (ObjectDisposedException) { }
            nextState = controller.ChaseState;
            shapeShiftDuration = controller.shapeShiftDuration;
            controller.DisableHpBar();
            controller.IsInvincible = true;
            controller.statusCondition.NonTarget.isActive = true;
            controller.animator.Play("Idle");
            SetTransformedPlayer();
            DisappaerWerewolf();
        }
        public override void OnUpdate()
        {
            if (isEndShapeAction)
            {
                //Debug.Log("ChaseState‚É‘JˆÚ‚µ‚Ü‚·");
                controller.ChangeState(nextState);
            }
        }
        public override void OnExit()
        {
            controller.statusCondition.NonTarget.isActive = false;
        }
        async void SetTransformedPlayerPrefab()
        {
            var transformedPlayerObj = await SetFieldFromAssets.SetField<GameObject>("Monsters/Werewolf/TransformedPlayer");
            if (transformedPlayerObj == null) return;
            transformedPlayerPrefab = transformedPlayerObj.GetComponent<TransformedPlayer.TransformedPlayer>();
        }
        async void DisappaerWerewolf()
        {
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    var color = mat.color;
                    color.a = 1.0f;
                    mat.color = color;
                    Debug.Log(mat.color);
                }
            }
            await controller.WaitFOAllMesh(shapeShiftDuration);
            controller.gameObject.SetActive(false);
            controller.IsInvincible = false;
        }
        async void SetTransformedPlayer()
        {
            var pos = controller.transform.position;
            var rot = controller.transform.rotation;
            var transformedPlayer = UnityEngine.Object.Instantiate(transformedPlayerPrefab, pos, rot);
            transformedPlayer.originalEntity = controller;
            transformedPlayer.transform.SetParent(controller.transform);
            transformedPlayer.isSummoned = true;
            transformedPlayer.ownerID = controller.ownerID;
            try
            {
                await UniTask.WaitUntil(() => !controller.IsInvincible,
                    cancellationToken: controller.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { return; }
            transformedPlayer.transform.SetParent(null);
            transformedPlayer.ReflectEachHP(controller.currentHP);
        }
        public async void EndShapeShiftAction()
        {
            controller.IsInvincible = true;
            controller.gameObject.SetActive(true);
            await controller.WaitFIAllMesh(shapeShiftDuration);
            controller.meshMaterials.ForEach(mats =>
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    FadeProcessHelper.ChangeToOpaque(mat);
                }
            });
            controller.IsInvincible = false;
            isEndShapeAction = true;
        }
    }
}


