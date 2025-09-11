using UnityEngine;

[CreateAssetMenu]
public class ContinuousAttackMonsterStatus : MonsterStatusData
{
    [SerializeField] ContinuousAttackInfo continuousAttackInfo;

    public ContinuousAttackInfo _ContinuousAttackInfo => continuousAttackInfo;

    [System.Serializable]
    public class ContinuousAttackInfo
    {
        [SerializeField] int continuousCount;

        public int ContinuousCount  => continuousCount;
    }
}
