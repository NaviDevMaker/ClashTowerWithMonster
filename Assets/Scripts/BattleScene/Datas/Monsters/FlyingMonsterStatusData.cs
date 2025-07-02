using UnityEngine;
[CreateAssetMenu]
public class FlyingMonsterStatusData : MonsterStatusData
{
    [SerializeField] float flyingOffsetY;
    public float FlyingOffsetY => flyingOffsetY;
}
