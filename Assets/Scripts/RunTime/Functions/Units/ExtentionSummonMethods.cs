using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Monsters;
using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class ExtentionSummonMethods
{
    public static void SetSummonParticle<T>(this T controller,Vector3 particlePos) where T : MonsterControllerBase<T>
    {
        controller.StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(particlePos, CardType.Monster));
    }
    public static async void SummonMoveAction(this MonoBehaviour controller,float offsetY = 8.0f)
    {
        try
        {
            Debug.Log("—Ž‰º‚µ‚Ü‚·");
            var targetPos = controller.transform.position;
            var appearPos = targetPos + Vector3.up * offsetY;
            controller.transform.position = appearPos;
            var moveSpeed = 30.0f;
            var originalScale = controller.transform.localScale;
            var scale = new Vector3(originalScale.x * 0.3f, originalScale.y * 3f, originalScale.y * 0.3f);
            controller.transform.localScale = scale;
            while ((controller.transform.position - targetPos).magnitude > 0.1f)
            {
                var currentPos = controller.transform.position;
                var move = Vector3.MoveTowards(currentPos, targetPos, Time.deltaTime * moveSpeed);
                controller.transform.position = move;

                if (controller.transform.position.y <= targetPos.y)
                {
                    controller.transform.position = targetPos;
                    break;
                }
                await UniTask.Yield(cancellationToken: controller.GetCancellationTokenOnDestroy());
            }

            var sequence = DOTween.Sequence();
            var targetScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
            var duration = 0.2f;
            sequence.Append(controller.transform.DOScale(targetScale, duration))
                .Append(controller.transform.DOScale(originalScale, duration));
        }
        catch (OperationCanceledException) { throw; }
    }
}

