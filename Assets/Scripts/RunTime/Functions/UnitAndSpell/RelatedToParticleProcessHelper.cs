using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public static class RelatedToParticleProcessHelper
{
    public static async UniTask  WaitUntilParticleDisappear(ParticleSystem particle)
    {
        if (particle == null) return;
        try
        {
            while (particle.IsAlive())
            {
                await UniTask.Yield(cancellationToken: particle.GetCancellationTokenOnDestroy());
            }
        }
        catch (OperationCanceledException) {}
    }
}
