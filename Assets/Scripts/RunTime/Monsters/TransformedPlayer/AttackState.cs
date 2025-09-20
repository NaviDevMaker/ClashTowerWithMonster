using UnityEngine;

namespace Game.Monsters.TransformedPlayer
{
    public class AttackState : AttackStateBase<TransformedPlayer>
    {
        public AttackState(TransformedPlayer controller) : base(controller) { }

        public override void OnEnter()
        {
            base.OnEnter();
            CastShadowChange(isAppearMyShadow:true);
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<TransformedPlayer >(controller, this, clipLength,7,
                controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            CastShadowChange(isAppearMyShadow:false);
            base.OnExit();
        }

        void CastShadowChange(bool isAppearMyShadow)
        {
             controller.AllMesh.ForEach(mesh =>
             {
                 mesh.shadowCastingMode = isAppearMyShadow ? UnityEngine.Rendering.ShadowCastingMode.On
                                          : UnityEngine.Rendering.ShadowCastingMode.Off;
             });
                
             controller.originalEntity.AllMesh.ForEach(mesh =>
             {
                 mesh.shadowCastingMode = isAppearMyShadow ? UnityEngine.Rendering.ShadowCastingMode.Off
                                          : UnityEngine.Rendering.ShadowCastingMode.On;
             });
        }
    }
}