using Game.Monsters;
using UnityEngine;

[CreateAssetMenu]
public class MultiSpawnMonsterData  : ScriptableObject
{
    [SerializeField] int spawnCount;
    [SerializeField] float eachSpawnDelayTime;
    [SerializeField] GameObject monsterPrefab;

    public int SpawnCount  => spawnCount;
    public float EachSpawnDelayTime  => eachSpawnDelayTime;

    public GameObject MonsterPrefab => monsterPrefab;
}
