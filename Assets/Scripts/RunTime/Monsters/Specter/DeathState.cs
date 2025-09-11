using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Game.Monsters.Specter
{
    public class DeathState : DeathStateBase<SpecterController>
    {
        public DeathState(SpecterController controller) : base(controller) => SetDeathSpecter();

        GameObject deathSpecter;
        public Func<bool> getDeathMoverStatus;
        public override async void OnEnter()
        {
            base.OnEnter();
            var pos = controller.transform.position;
            var rot = controller.transform.rotation;
            var spawnedSpecter = UnityEngine.Object.Instantiate(deathSpecter,pos,rot);
            var cmp = spawnedSpecter.GetComponent<DeathSpecterMover>();
            getDeathMoverStatus = () => cmp.isReachedTargetPos;
            cmp.Initialize(controller);
            var duration = 2.0f;
            await UniTask.Delay(TimeSpan.FromSeconds(duration),cancellationToken:controller.GetCancellationTokenOnDestroy());
            cmp.isDamageable = true;
            cmp.StartDeathSpecterAction();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }          
        async void SetDeathSpecter() => deathSpecter = await SetFieldFromAssets.SetField<GameObject>("Specter/DeathSpecter");
    }
}