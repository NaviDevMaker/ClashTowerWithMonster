using UnityEngine;

public class StatusData : ScriptableObject
{
    
    [SerializeField] int attackAmount;
    [SerializeField] int hp;
    [SerializeField] float pushAmount;

    public int AttackAmount { get => attackAmount;}
    public int Hp { get => hp;}
    public float PushAmount { get => pushAmount;}
}
