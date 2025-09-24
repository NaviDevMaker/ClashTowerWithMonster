using UnityEngine;

public class TowerStatusData :StatusData
{
    [SerializeField] float moverSpeed;
    [SerializeField] float searchRadius;

    public float MoverSpeed { get => moverSpeed;}
    public float SearchRadius { get => searchRadius;}
}
