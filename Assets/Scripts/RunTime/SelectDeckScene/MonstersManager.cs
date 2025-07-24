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

    // Update is called once per frame
    void Update()
    {
        
    }

    async void Initialize()
    {
        try
        {
            stoneMaterial = await SetFieldFromAssets.SetField<Material>("");

        }
        catch(OperatorException )
        {
            Debug.LogWarning("Ç±ÇÃÉAÉhÉåÉXÇÕë∂ç›ÇµÇ‹ÇπÇÒ");
            return;
        }

        var monstersObj = (await SetFieldFromAssets.SetFieldByLabel<GameObject>("DeckChooseMonster")).ToList();
        foreach (GameObject monster in monstersObj)
        {
            var selectableMonster = monster.GetComponent<SelectableMonster>();
            selectableMonster.stoneMaterial = stoneMaterial;
        } 
    }
}
