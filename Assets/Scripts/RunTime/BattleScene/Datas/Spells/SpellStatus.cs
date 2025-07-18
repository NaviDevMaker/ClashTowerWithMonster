using System;
using UnityEngine;

[CreateAssetMenu]
public class SpellStatus : ScriptableObject
{
    [SerializeField] int effectAmount;
    [SerializeField] float pushAmount;
    [SerializeField] float perPushDurationAndStunTime;
    [SerializeField] SpellType spellType;
    public int EffectAmont => effectAmount;
    public float PushAmount => pushAmount;
    public float PerPushDurationAndStunTime => perPushDurationAndStunTime;
    public SpellType SpellType => spellType;
}

[Flags]
public enum SpellType
{ 
    Damage = 1 << 0,
    Heal = 1 << 1,
    DamageToEveryThing = 1 << 2,
    OtherToPlayerside = 1 << 3,
    OtherToEnemyside = 1 << 4,
    OtherToEverything = 1 << 5,
}

