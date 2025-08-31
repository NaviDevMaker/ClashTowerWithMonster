using UnityEngine;

namespace Game.Monsters.SpellDemon
{
    public class DeathState : DeathStateBase<SpellDemonController>
    {
        public DeathState(SpellDemonController controller) : base(controller) { }

        public override void OnEnter()
        {
            controller.transform.SetParent(null);

            Debug.Log($"{controller.animator.speed},éÄÇ ÉÇÅ[ÉVÉáÉìÇ…ì¸ÇËÇ‹Ç∑");
            SetNormalStatus();
            base.OnEnter();
            Debug.Log(clipLength);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        void SetNormalStatus()
        {
            controller.targetUnit.statusCondition.DemonCurse.isActive = false;
            controller.targetUnit.statusCondition.DemonCurse.isEffectedCount = 0;
        }
    }

}