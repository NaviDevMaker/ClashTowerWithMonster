using Game.Monsters;
using UnityEngine;

[CreateAssetMenu]
public class KingDragonStatus : TowerStatusData
{
    [SerializeField] KingDragonAnimPar animatorParemeterSet;
    [SerializeField] KingDragonFireMover kingDragonFireMover;
    [SerializeField] float attackSimpleRange;
    [SerializeField] float attackLongRange;
    public KingDragonAnimPar KingAnimPar  => animatorParemeterSet;
    public float AttackSimpleRange => attackSimpleRange;
    public float AttackLongRange => attackLongRange;

    public KingDragonFireMover KingDragonFireMover  => kingDragonFireMover;
}
