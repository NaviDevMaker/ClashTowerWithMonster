using UnityEngine;

[CreateAssetMenu]
public class SpellStatus : ScriptableObject
{
    [SerializeField] int damage;
    [SerializeField] float pushAmount;
    [SerializeField] float spellDuration;
    public int Damage => damage;
    public float PushAmount => pushAmount;
    public float SpellDuration => spellDuration;
}
