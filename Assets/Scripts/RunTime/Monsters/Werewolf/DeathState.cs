using UnityEngine;

namespace Game.Monsters.Werewolf
{
    public class DeathState : DeathStateBase<WerewolfController>
    {
        public DeathState(WerewolfController controller) : base(controller) { }
        public override void OnEnter()
        {
            MaterialsProcess();
            base.OnEnter();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        void MaterialsProcess()
        {
            controller.meshMaterials.ForEach(mats =>
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    var mat = mats[i];
                    var color = mat.color;
                    color.a = 1.0f;
                    mat.color = color;
                }
            });
        }
    }
}