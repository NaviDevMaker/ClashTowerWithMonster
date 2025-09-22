using UnityEngine;

[CreateAssetMenu]
public class KingDragonStatus : TowerStatusData
{
    [SerializeField] KingDragonAnimPar animatorParemeterSet;
    [SerializeField] float attackSimpleRange;
    [SerializeField] float attackLongRange;
    public KingDragonAnimPar KingAnimPar  => animatorParemeterSet;
    public float AttackSimpleRange => attackSimpleRange;
    public float AttackLongRange => attackLongRange;
}
