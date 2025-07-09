using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SelectableMonstersList : ScriptableObject
{
    [SerializeField] List<UnitBase> selectableMonsters;

    public List<UnitBase> SelectableMonsters => selectableMonsters;
}
