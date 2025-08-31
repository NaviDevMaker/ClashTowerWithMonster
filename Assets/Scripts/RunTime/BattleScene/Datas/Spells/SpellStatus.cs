using System;
using UnityEngine;

[CreateAssetMenu]
public class SpellStatus : ScriptableObject
{
    [SerializeField] int effectAmount;
    [SerializeField] float pushAmount;
    [SerializeField] float perPushDurationAndStunTime;
    [SerializeField] SpellType spellType;
    [SerializeField] SpellInvokeType invokeType;
    [SerializeField] float spellDuration;
    [SerializeField] SpecificSpellType specificSpellType;
    [SerializeField] TargetCount targetCount;
    public int EffectAmont => effectAmount;
    public float PushAmount => pushAmount;
    public float PerPushDurationAndStunTime => perPushDurationAndStunTime;
    public SpellType SpellType => spellType;
    public SpellInvokeType InvokeType => invokeType;
    public float SpellDuration { get => spellDuration; set => spellDuration = value; }
    public TargetCount TargetCount => targetCount;
    public SpecificSpellType SpecificSpellType  => specificSpellType;
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
[Flags]
public enum SpellInvokeType
{
    Instant,
    CastTime,
    Continuous
}
[Flags]
public enum SpecificSpellType
{ 
    Freeze = 1 << 0,
    Confusion = 1 << 1,
    ShapeShift = 1 << 2,
}

[Flags]
public enum TargetCount
{ 
    InRange = 1 << 0,
    One = 1 << 1,
    Two = 1 << 2,
    Three = 1 << 3,
}





