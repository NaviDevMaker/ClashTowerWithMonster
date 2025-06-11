using UnityEngine;

[CreateAssetMenu]
public class SpellStatus : ScriptableObject
{
    [SerializeField] int damage;
    [SerializeField] float pushAmount;

    public int Damage => damage;
    public float PushAmount => pushAmount;
}
