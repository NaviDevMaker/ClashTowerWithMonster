using UnityEngine;

public interface IFlying
{
    float FlyingOffsetY { get;}
}
[CreateAssetMenu]
public class FlyingMonsterStatusData : MonsterStatusData,IFlying
{
    [SerializeField] float flyingOffsetY;
    public float FlyingOffsetY => flyingOffsetY;
}
