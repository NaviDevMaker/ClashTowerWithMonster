using Game.Monsters;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public static class UnitManager
{
    public static List<UnitBase> InstanciatedMonster = new List<UnitBase>(); 

    public static void AddToList(params UnitBase[] unitBases)
    {
        if (SceneManager.GetActiveScene().name != "DeckChooseScene") return;
        InstanciatedMonster.AddRange(unitBases);
    }
    public static void DestroyAll()
    {
        foreach (var monster in InstanciatedMonster) monster.isDead = true;
        InstanciatedMonster.Clear();
        InstanciatedMonster.TrimExcess();
    }
}
