using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Monsters.Orc
{
    public class AttackState : AttackStateBase<OrcController>,IEffectSetter
    {
        public AttackState(OrcController controller) : base(controller)
        {
            SetEffect();
        }
        ParticleSystem attackEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<OrcController >(controller, this, clipLength,13,
                controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        protected override async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        {
            var arguments = new SimpleAttackArguments
            { 
               getTargets = attackArguments.getTargets,
               attackEffectAction = PlayAttackEffect,
               specialEffectAttack = controller.orcWeponPusher.PushUnitToRight
            };
            await base.Attack_Generic(arguments);
        }
        async void PlayAttackEffect()
        {
            var offsetY = Vector3.up * 1.5f;
            var offsetZ = controller.transform.forward * 1.0f;
            var pos = controller.transform.position + offsetY + offsetZ;
            var rot = controller.transform.rotation * attackEffect.transform.rotation;
            var effect = UnityEngine.Object.Instantiate(attackEffect,pos,rot);
            effect.Play();
            await RelatedToParticleProcessHelper.WaitUntilParticleDisappear(effect);
            UnityEngine.Object.Destroy(effect.gameObject);
        }
        public async void SetEffect()
        {
            var obj = await SetFieldFromAssets.SetField<GameObject>("Effects/OrcAttackEffect");
            if (obj == null) return;
            attackEffect = obj.GetComponent<ParticleSystem>();
        }
    }
}