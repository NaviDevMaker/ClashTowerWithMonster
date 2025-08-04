using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class LineUpFields
{
    [SerializeField] public float criterioX;
    [SerializeField] public float criterioZ;
    [SerializeField] public float spaceX;
    [SerializeField] public float spaceZ;
}
public class SelectablePrefabManager : MonoBehaviour
{
    public List<SelectableMonster> monsters { get; set; } = new List<SelectableMonster>();
    Material stoneMaterial;

    [SerializeField] LineUpFields lineUpFields; 
    int line = 0;
    int columCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public async void Initialize()
    {
        await SetStoneMaterial();
        SetMonster();
    }

    async void SetMonster()
    {
        List<GameObject> monsterObjs = new List<GameObject>();
        try
        {
            monsterObjs = (await SetFieldFromAssets.SetFieldByLabel<GameObject>("DeckChooseMonster")).ToList();
        }
        catch (OperatorException)
        {
            Debug.LogWarning("このアドレスは存在しません");
            return;
        }

        var mosntersFromAddress = new List<SelectableMonster>();
        foreach (GameObject monster in monsterObjs)
        {
            var monsterObj = Instantiate(monster);
            var selectableMonster = monsterObj.GetComponent<SelectableMonster>();
            mosntersFromAddress.Add(selectableMonster);
        }
        monsters = mosntersFromAddress.OrderBy(monster => monster.sortOrder).ToList();

        var cardDatas = SelectableCardManager.Instance.allCardDatas;
        for (var i = 0; i < cardDatas.Count;i++)
        {
            var cardType = cardDatas[i].CardType;
            var monster = monsters[i];
            monster.cardType = cardType;
        }
        MonsterLineUp();
    }
    async UniTask SetStoneMaterial()
    {
        try
        {
            stoneMaterial = await SetFieldFromAssets.SetField<Material>("Materiials/StoneShader");

        }
        catch (OperatorException)
        {
            Debug.LogWarning("このアドレスは存在しません");
            return;
        }
    }
    void MonsterLineUp()
    {
        Debug.Log($"Lineは{line},コラムは{columCount}");
        var criterioX = lineUpFields.criterioX;
        var criterioZ = lineUpFields.criterioZ;
        var spaceX = lineUpFields.spaceX;//25f
        var spaceZ = lineUpFields.spaceZ;//10f
        Debug.Log($"{criterioX},{criterioZ},{spaceX},{spaceZ}");
        var index = 0;
        var count = 0;
        for (int l = 0; l < line; l++)
        {
            for (int c = 0; c < columCount; c++)
            {
                Debug.Log($"{index}aaaaa");
                count++;
                var selectableMonster = monsters[index];
                var pos = new Vector3(criterioX + spaceX * c,0f,criterioZ - spaceZ * l);
                pos.y = Terrain.activeTerrain.SampleHeight(pos);
                selectableMonster.transform.position = pos;
                selectableMonster.stoneMaterial = stoneMaterial;
                selectableMonster.Initialize();
                if (count == monsters.Count) break;
                index++;
            }
        }
    }
    public void SetLine(int line,int columCount)
    {
        this.columCount = columCount;
        this.line = line;
    }
}
