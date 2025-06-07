using UnityEngine;

public class StatusData : ScriptableObject
{
    
    [SerializeField] int attackAmount;
    [SerializeField] int hp;

    public int AttackAmount { get => attackAmount;}
    public int Hp { get => hp;}
}
