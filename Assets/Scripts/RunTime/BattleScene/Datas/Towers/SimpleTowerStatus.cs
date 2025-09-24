using UnityEngine;

[CreateAssetMenu]
public class SimpleTowerStatus : TowerStatusData
{
    [SerializeField] float shotDuration;
    [SerializeField] GunMover gunMover;

    public float ShotDuration  => shotDuration;
    public GunMover GunMover => gunMover;
}
