using Cysharp.Threading.Tasks;
using UnityEngine;

public static class RelatedToParticleProcessHelper
{
    public static async UniTask  WaitUntilParticleDisappear(ParticleSystem particle)
    {
        if (particle == null) return;
        while (particle.IsAlive())
        {
            await UniTask.Yield();
        }
    }
}
