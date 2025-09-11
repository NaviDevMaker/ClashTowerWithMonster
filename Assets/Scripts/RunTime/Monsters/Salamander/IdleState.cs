using UnityEngine;
using Cysharp.Threading.Tasks;
namespace Game.Monsters.Salamander
{
    public class IdleState : IdleStateBase<SalamanderController>
    {
        public IdleState(SalamanderController controller) : base(controller) { }

        public override void OnEnter()
        {
            OnEnterProcess().Forget();
        }
        public override void OnUpdate()
        {
           base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask OnEnterProcess()
        {
            var summonFireObj = await SetFieldFromAssets.SetField<GameObject>("Monsters/SalamanderSummonFireEffect");
            if (summonFireObj == null) return;
            var pos = controller.transform.position;
            var rot = controller.transform.rotation;
            var summonFire = UnityEngine.Object.Instantiate(summonFireObj,pos,rot);
            var cmp = summonFire.GetComponent<SummonFireLing>();
            if (cmp != null)
            {
                cmp.Initialize(controller);
                cmp.DamageAndPush();
            }
            await base.OnEnterProcess();
        }
    }
}