using UnityEngine;

[CreateAssetMenu]
public class SpellStatus : ScriptableObject
{
    [SerializeField] float damage;
    [SerializeField] float pushAmount;

    public float Damage => damage;
    public float PushAmount => pushAmount;
}
