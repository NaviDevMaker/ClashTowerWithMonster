using UnityEngine;

[CreateAssetMenu]
public class ContinuousAttackMonsterStatus : MonsterStatusData
{
    [SerializeField] ContinuousAttackInfo continuousAttackInfo;

    public ContinuousAttackInfo _ContinuousAttackInfo => continuousAttackInfo;

    [System.Serializable]
    public class ContinuousAttackInfo
    {
        [SerializeField] float interval;
        public float Interval => interval;
    }
}
