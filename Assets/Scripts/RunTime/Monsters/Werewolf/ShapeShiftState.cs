using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

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
        public bool isShaping { get; private set; } = false;
        public override async void OnEnter()
        {
            isShaping = true;
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
            await DisappaerWerewolf();
            SetTransformedPlayer();
        }
        public override void OnUpdate()
        {
            if (isEndShapeAction)
            {
                //Debug.Log("ChaseStateに遷移します");
                controller.ChangeState(nextState);
            }
        }
        public override void OnExit()
        {
            controller.statusCondition.NonTarget.isActive = false;
            isShaping = false;
        }
        async void SetTransformedPlayerPrefab()
        {
            var transformedPlayerObj = await SetFieldFromAssets.SetField<GameObject>("Monsters/Werewolf/TransformedPlayer");
            if (transformedPlayerObj == null) return;
            transformedPlayerPrefab = transformedPlayerObj.GetComponent<TransformedPlayer.TransformedPlayer>();
        }
        async UniTask DisappaerWerewolf()
        {
            var type = controller.GetType();
            while(type != null && type.BaseType != typeof(MonoBehaviour)) type = type.BaseType;
            var prop = type.GetField("originalMaterialColors",BindingFlags.NonPublic | BindingFlags.Instance);
            var originalMaterialColors = prop.GetValue(controller) as List<Color[]>;
            for (int i = 0; i < controller.meshMaterials.Count; i++)
            {
                var materials = controller.meshMaterials[i];
                var colors = originalMaterialColors[i];
                for (int j = 0; j < materials.Length; j++)
                {
                    var mat = materials[j];
                    var color = colors[j];
                    color.a = 1.0f;
                    mat.color = color;
                    Debug.Log(mat.color);
                }
            }
            var originalScale = controller.transform.lossyScale;
            var originalRot = controller.transform.rotation;
            var seq = controller.GetTransformSequence(shapeShiftDuration);
            var seqTask = seq.ToUniTask(cancellationToken:controller.GetCancellationTokenOnDestroy());
            controller.ShapeEffectAction().Forget();
            await UniTask.WhenAll(controller.WaitFOAllMesh(shapeShiftDuration),seqTask);

            controller.transform.localScale = originalScale;
            controller.transform.localRotation = originalRot;
        }
       
        async void SetTransformedPlayer()
        {
            var pos = controller.transform.position;
            var rot = controller.transform.rotation;
            var transformedPlayer = UnityEngine.Object.Instantiate(transformedPlayerPrefab, pos, rot);
            try
            {
                //transformedPlayerのInitializedでのHPの処理がStartだからそれが終わる前に
                //transformedPlayer.ReflectEachHP(controller.currentHP);が動作してしまい、
                //MaxのHPがあとから上書きされて意図した動作にならないから1フレーム待って、正しい処理順序になるようにする
                await UniTask.Yield(cancellationToken: controller.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { }
            transformedPlayer.originalEntity = controller;
            //transformedPlayer.transform.SetParent(controller.transform);
            controller.transform.SetParent(transformedPlayer.transform);
            transformedPlayer.isSummoned = true;
            transformedPlayer.ownerID = controller.ownerID;
            //transformedPlayer.transform.SetParent(null);
            transformedPlayer.ReflectEachHP(controller.currentHP);
        }
        public async void EndShapeShiftAction()
        {
            var animPar = controller.MonsterAnimPar;
            controller.animator.SetBool(animPar.Attack_Hash, false);
            controller.animator.SetBool(animPar.Chase_Hash, false);
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


