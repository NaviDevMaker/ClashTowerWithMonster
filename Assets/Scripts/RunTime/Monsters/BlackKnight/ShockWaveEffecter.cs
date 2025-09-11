using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;
using static UnityEngine.UI.GridLayoutGroup;

namespace Game.Monsters.BlackKnight
{
    public class ShockWaveEffecter : PushableWeponBase<BlackKnightController>, IRangeWeponAddForce<BlackKnightController>
    {
        public AddForceToUnit<PushableWeponBase<BlackKnightController>> _addForceToUnit { get; set; }
        public override void Initialize(BlackKnightController owner)
        {
            ownerID = owner.ownerID;
            moveType = owner.moveType;
            UnitScale = UnitScale.middle;
            base.Initialize(owner);
            var rangeInfo = weponOwner.RangeAttackMonsterStatusData._RangeAttackInfo;
            var pushAmount = rangeInfo.PushAmount;
            var pushDuration = rangeInfo.PerPushDuration;
            _addForceToUnit = new AddForceToUnit<PushableWeponBase<BlackKnightController>>(this, pushAmount, pushDuration);
        }
        public void PushUnit(UnitBase target)
        {
            Debug.Log($"{this},ÉfÅ[ÉÇÉìÇ™âüÇµÇ‹Ç∑");
            _addForceToUnit.CompareEachUnit(target);

        }
    }
}

