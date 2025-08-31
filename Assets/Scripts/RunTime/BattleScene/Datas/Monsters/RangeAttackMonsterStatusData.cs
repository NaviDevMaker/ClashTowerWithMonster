using UnityEngine;

[CreateAssetMenu]
public class RangeAttackMonsterStatusData : MonsterStatusData
{
    [SerializeField] RangeAttackInfo rangeAttackInfo;

    public RangeAttackInfo _RangeAttackInfo => rangeAttackInfo;

    [System.Serializable]   
    public class RangeAttackInfo
    {
        [SerializeField] GameObject rangeAttackWepon;
        [SerializeField] float pushAmount;
        [SerializeField] float perPushDuration;
        public GameObject RangeAttackWepon => rangeAttackWepon;
        public float PushAmount => pushAmount;
        public float PerPushDuration  => perPushDuration;
    } 
}
