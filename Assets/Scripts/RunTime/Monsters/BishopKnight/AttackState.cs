using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.BishopKnight
{
    public class AttackState : AttackStateBase<BishopKnightController>
    {
        public AttackState(BishopKnightController controller) : base(controller) { }

        ParticleSystem tornadoEffect = null;
        public override void OnEnter()
        {
            base.OnEnter();
            TornadoEffectSet();

            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<BishopKnightController >(controller, this, clipLength,15,
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
        protected override async UniTask Attack_Generic(Func<List<UnitBase>> getTargets,
            UnityAction<UnitBase> statusConditionAttack = null, UnityAction continuousAttack = null)
        {
           GameObject tornadoObj = null;
           PlayTornadoParticle(out tornadoObj);
           await base.Attack_Generic(getTargets,statusConditionAttack);
           DestroyTornado(tornadoObj);
        }
        async void TornadoEffectSet()
        {
            if (tornadoEffect != null) return;
            var tornadoObj = await SetFieldFromAssets.SetField<GameObject>("Effects/Tornado");
            if(tornadoObj == null) return;  
            tornadoEffect = tornadoObj.GetComponent<ParticleSystem>();
            var main = tornadoEffect.main;
            var animSpeed = controller.MonsterStatus.AnimaSpeedInfo.AttackStateAnimSpeed;
            main.duration = clipLength / animSpeed;
        }
        void PlayTornadoParticle(out GameObject tornadoObj)
        {
            if (tornadoEffect == null)
            {
                tornadoObj = null;
                return;
            }
            var tornado = UnityEngine.Object.Instantiate(tornadoEffect);
            tornado.transform.SetParent(controller.wepon.transform);
            tornado.transform.localPosition = Vector3.zero;
            tornado.transform.localRotation = Quaternion.identity;
            tornadoObj = tornado.gameObject;
            tornado.Play();
        }
        async void DestroyTornado(GameObject tornadoObj)
        {
            if (tornadoObj == null) return;
            var p = tornadoObj.GetComponent<ParticleSystem>();
            var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
            await task;
            if(p != null) UnityEngine.Object.Destroy(p.gameObject);
        }
    }
}