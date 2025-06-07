using UnityEngine;

[CreateAssetMenu]
public class TowerStatusData :StatusData
{
    [SerializeField] float gunSpeed;
    [SerializeField] float shotDuration;
    [SerializeField] GunMover towerShotgun;
    [SerializeField] float searchRadius;

    public float GunSpeed { get => gunSpeed;}
    public float ShotDuration { get => shotDuration;}
    public GunMover TowerShotgun { get => towerShotgun; }
    public float SearchRadius { get => searchRadius;}
}
