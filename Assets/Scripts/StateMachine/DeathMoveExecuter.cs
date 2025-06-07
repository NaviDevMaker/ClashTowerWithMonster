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
        
        var meshesQueue = new Queue<Renderer>(); 
        
        if (monsterController.MySkinnedMeshes.Count != 0) monsterController.MySkinnedMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));
        if (monsterController.MyMeshes.Count != 0) monsterController.MyMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));

        monsterController.animator.SetTrigger(monsterController.MonsterAnimPar.Death);
        var stateName = monsterController.MonsterAnimPar.deathAnimClipName;
        await UniTask.WaitUntil(() => monsterController.animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));
        var cts = new CancellationTokenSource();
        var clipLengthBySpeed = clipLength * (1 / monsterController.animator.speed) * (1 / stateAnimSpeed);

        foreach (var mesh in meshesQueue)
        {
            var material = mesh.material;
            FadeOutColor(clipLengthBySpeed, cts.Token, material);
        }

        EffectManager.Instance.deathEffect.GenerateDeathEffect<UnitBase>(monsterController, clipLengthBySpeed);
        monsterController.EnableHpBar();
        await UniTask.Delay(TimeSpan.FromSeconds(clipLengthBySpeed));
        cts.Cancel();
        cts.Dispose();
        //controller.DestroyAll();
    }
    public async UniTask ExecuteDeathAction_Player<T>(PlayerControllerBase<T> playerController, float clipLength, float stateAnimSpeed) where T : PlayerControllerBase<T>
    {

        var meshesQueue = new Queue<Renderer>();

        if (playerController.MySkinnedMeshes.Count != 0) playerController.MySkinnedMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));
        if (playerController.MyMeshes.Count != 0) playerController.MyMeshes.ForEach(mesh => meshesQueue.Enqueue(mesh));

        playerController.animator.SetTrigger(playerController.AnimatorPar.Death);
        var stateName = playerController.AnimatorPar.deathAnimClipName;
        await UniTask.WaitUntil(() => playerController.animator.GetCurrentAnimatorStateInfo(0).IsName(stateName));
        var cts = new CancellationTokenSource();
        var clipLengthBySpeed = clipLength * (1 / playerController.animator.speed) * (1 / stateAnimSpeed);

        foreach (var mesh in meshesQueue)
        {
            var material = mesh.material;
            FadeOutColor(clipLengthBySpeed, cts.Token, material);
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
        var material = archer.MyMesh.material;
        FadeOutColor(clipLengthBySpeed,token, material);
        EffectManager.Instance.deathEffect.GenerateDeathEffect(archer, clipLength);
        await UniTask.Delay(TimeSpan.FromSeconds(clipLength));
        cts.Cancel();
        cts.Dispose();
        UnityEngine.Object.Destroy(archer.gameObject);
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
