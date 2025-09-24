using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.Specter
{
    public class ChaseState : ChaseStateBase<SpecterController>
    {
        public ChaseState(SpecterController controller) : base(controller) { }
        float translusent = 0.3f;
        ParticleSystem runParticle;
        public override void OnEnter()
        {
            runParticle = controller.GetComponentInChildren<ParticleSystem>();
            if (runParticle != null) runParticle.Play();
            controller.statusCondition.Transparent.isActive = true;
            AlphaChange();
            base.OnEnter();
        }
        public override void OnUpdate(){ base.OnUpdate(); }
        public override void OnExit()
        {
            if (runParticle != null) runParticle.Stop();
            controller.statusCondition.Transparent.isActive = false;
            AlphaChange();
            base.OnExit();
        }
        void AlphaChange()
        {
            var isTransparent = controller.statusCondition.Transparent.isActive;
            UnityAction<Material> typeChangeAction = (mat) =>
            {
                if (isTransparent) FadeProcessHelper.ChangeToTranparent(mat);
                else FadeProcessHelper.ChangeToOpaque(mat);
            };
            controller.meshMaterials.ForEach(materials =>
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    var mat = materials[i];
                    typeChangeAction(mat);
                    var color = mat.color;
                    color.a = isTransparent ? translusent : 1.0f;
                    mat.color = color;
                }
            });
        }
    }
}