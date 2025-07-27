using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MonstersManager : MonoBehaviour
{
    List<SelectableMonster> monsters = new List<SelectableMonster>();
    Material stoneMaterial;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        SetStoneMaterial();
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
        foreach (GameObject monster in monsterObjs)
        {
            var monsterObj = Instantiate(monster);
            var selectableMonster = monsterObj.GetComponent<SelectableMonster>();
            selectableMonster.Initialize(stoneMaterial);
            monsters.Add(selectableMonster);
        }
    }
    async void SetStoneMaterial()
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
}
