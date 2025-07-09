using Game.Spells;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForExefile : MonoBehaviour
{
    float time = 0f;
    float spawnTime = 3.0f;

    float maxX = 0f;
    float maxZ = 0f;
    int maxCount = 10;
    [SerializeField] List<GameObject> prefabs;
    List<GameObject> initializedObjes = new List<GameObject>();
    // Update is called once per frame

    private void Start()
    {
        prefabs.ForEach(prefab =>
        {
            var obj = Instantiate(prefab,new Vector3(1000,0,0), prefab.transform.rotation);
            if (obj.TryGetComponent<UnitBase>(out var unit)) unit.gameObject.SetActive(false);
            initializedObjes.Add(obj);
        });
        Terrain terrain = Terrain.activeTerrain;
        maxX = terrain.terrainData.size.x + terrain.transform.position.x;
        maxZ = terrain.terrainData.size.z + terrain.transform.position.z;
    }
    void Update()
    {
        time += Time.deltaTime;
        if(time >= spawnTime)
        {
            if (UnitCount() >= maxCount) return;
            time = 0f;
            Spawn();
        }
    }

    int UnitCount()
    {
        var units = GameObject.FindObjectsByType<UnitBase>(sortMode: FindObjectsSortMode.None);
        var sortArray = units.Where(unit =>
        {
            var side = unit.GetUnitSide(unit.ownerID);
            var notTower = unit.UnitType != UnitType.tower;
            return side == Side.EnemySide; //&& notTower;
        }).ToArray();
        return sortArray.Length;
    }
    void Spawn()
    {
        var random = Random.Range(0, initializedObjes.Count);
        var prefab = initializedObjes[random];
        var x = Random.Range(0, maxX);
        var z = Random.Range(0, maxZ);
        var pos = new Vector3(x,0f, z);
        pos.y = Terrain.activeTerrain.SampleHeight(pos) + 0.5f;
        var obj = Instantiate(prefab, pos, prefab.transform.rotation);
        obj.gameObject.SetActive(true);
        if(obj.TryGetComponent<UnitBase>(out var unit))
        {
            unit.ownerID = 1;
            //unit.Side = Side.EnemySide;
            StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(pos, CardType.Monster));
        }
        else if(obj.TryGetComponent<SpellBase>(out var spell))
        {
            StartCoroutine(EffectManager.Instance.magicCircleEffect.SummonEffect(pos, CardType.Spell));
        }

        if (obj.TryGetComponent<ISummonbable>(out var summonbable)) summonbable.isSummoned = true;
    }
}
