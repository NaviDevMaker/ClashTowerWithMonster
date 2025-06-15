using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Game.Monsters;
using Game.Players;
using System.Runtime.InteropServices;
using Game.Monsters.Archer;
using UnityEngine.Animations;
using Unity.VisualScripting;

public class DeathMoveExecuter
{

    public async UniTask ExecuteDeathAction_Monster<T>(MonsterControllerBase<T> monsterController,float clipLength,float stateAnimSpeed) where T:MonsterControllerBase<T> 
    {
        
        //var meshesQueue = new Queue<Renderer>(); 
        
        //if (monsterController.MySkinnedMeshes.Count != 0) monsterController.MySkinnedMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));
        //if (monsterController.MyMeshes.Count != 0) monsterController.MyMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));

        monsterController.animator.SetTrigger(monsterController.MonsterAnimPar.Death);
        var stateName = monsterController.MonsterAnimPar.deathAnimClipName;
        await UniTask.WaitUntil(() => monsterController.animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));
        var cts = new CancellationTokenSource();
        var clipLengthBySpeed = clipLength * (1 / monsterController.animator.speed) * (1 / stateAnimSpeed);


        foreach (var materials in monsterController.meshMaterials)
        {
            foreach (var material in materials)
            {
                FadeOutColor(clipLengthBySpeed, cts.Token, material);
            }
        }

        EffectManager.Instance.deathEffect.GenerateDeathEffect<UnitBase>(monsterController, clipLengthBySpeed);
        monsterController.EnableHpBar();
        await UniTask.Delay(TimeSpan.FromSeconds(clipLengthBySpeed));
        cts.Cancel();
        cts.Dispose();
        monsterController.DestroyAll();
    }
    public async UniTask ExecuteDeathAction_Player<T>(PlayerControllerBase<T> playerController, float clipLength, float stateAnimSpeed) where T : PlayerControllerBase<T>
    {

        var meshesQueue = new Queue<Renderer>();

        //if (playerController.MySkinnedMeshes.Count != 0) playerController.MySkinnedMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));
        //if (playerController.MyMeshes.Count != 0) playerController.MyMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));

        playerController.animator.SetTrigger(playerController.AnimatorPar.Death);
        var stateName = playerController.AnimatorPar.deathAnimClipName;
        await UniTask.WaitUntil(() => playerController.animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));
        var cts = new CancellationTokenSource();
        var clipLengthBySpeed = clipLength * (1 / playerController.animator.speed) * (1 / stateAnimSpeed);

        foreach (var materials in playerController.meshMaterials)
        {
            foreach (var material in materials)
            {
                FadeOutColor(clipLengthBySpeed, cts.Token, material);
            }
        }

        EffectManager.Instance.deathEffect.GenerateDeathEffect<UnitBase>(playerController, clipLengthBySpeed);
        playerController.EnableHpBar();
        await UniTask.Delay(TimeSpan.FromSeconds(clipLengthBySpeed));
        cts.Cancel();
        cts.Dispose();
        //controller.DestroyAll();
    }

    public async UniTask ExecuteDeathAction_Archer(ArcherController archer, float clipLength, float stateAnimSpeed)
    {

        archer.animator.SetTrigger(archer.death);

        await UniTask.WaitUntil(() => archer.animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));

        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var clipLengthBySpeed = clipLength * (1 / archer.animator.speed) * (1 / stateAnimSpeed);
        var mesh = archer.MyMesh;
        foreach (var material in mesh.materials)
        {
            FadeOutColor(clipLengthBySpeed, token, material);
        }
        EffectManager.Instance.deathEffect.GenerateDeathEffect(archer, clipLength);
        await UniTask.Delay(TimeSpan.FromSeconds(clipLength));
        cts.Cancel();
        cts.Dispose();
    }

    public async UniTask ExecuteDeathAction_Tower(TowerControlller tower, float length)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var mesh = tower.MyMeshes[0];
        for (int i = 0; i < mesh.materials.Length;i++)
        {
             var material = mesh.materials[i];
             //var material = new Material(originalMaterial);
            if (material.name.StartsWith("Rock_Light"))
            {
                Debug.Log(material.name.Trim());
                if (material.HasProperty("_Surface"))
                {
                    Debug.Log("プロパティを発見しました");
                    // SurfaceType = Transparent 相当の設定
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_Surface", 1); // 0:Opaque, 1:Transparent（URP用）
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetFloat("_ZWrite", 0); // 透過オブジェクトはZWriteを切る
                }
                else
                {
                    Debug.LogWarning("it has no existing proparty!!");
                }
            }
            Debug.Log(length);
             FadeOutColor(length, cts.Token,material);
        }
        EffectManager.Instance.deathEffect.GenerateDeathEffect(tower, length);
        tower.EnableHpBar();
        await UniTask.Delay(TimeSpan.FromSeconds(length));
        cts.Cancel();
        cts.Dispose();
        tower.DestroyAll();
    }
    async void FadeOutColor(float clipLengthBySpeed, CancellationToken cancellationToken, Material material)
    {
        Debug.Log("Playerのフェイドアウト開始");
        var time = 0f;
        var meshMaterial = material;

     
        var startColor = meshMaterial.color;
        var startAlpha = startColor.a;

        try
        {
            while (time <= clipLengthBySpeed && !cancellationToken.IsCancellationRequested)
            {
                var lerpedTime = time / clipLengthBySpeed;
                var color = startColor;
                color.a = Mathf.Lerp(startAlpha, 0f, lerpedTime);

                meshMaterial.color = color;
                //Debug.Log(meshMaterial.color.a);
                time += Time.deltaTime;
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("色変更キャンセルされました");
        }
        finally
        {
            var finalColor = startColor;
            finalColor.a = 0f;
            meshMaterial.color = finalColor;
            Debug.Log(meshMaterial.color.a);
        }
    }
}
