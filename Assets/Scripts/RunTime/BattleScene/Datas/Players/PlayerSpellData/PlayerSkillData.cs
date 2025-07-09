using System;
using UnityEngine;

[CreateAssetMenu]
public class PlayerSkillData : ScriptableObject
{
    [SerializeField] GameObject skillObj;
    [SerializeField] int energy;
    [SerializeField] float skillDuration;
    [SerializeField] int skillEffectAmount;
    [SerializeField] float skillPushAmount;
    [SerializeField] float perPushDurationAndStunTime;
    [SerializeField] SpellType spellType;
    public GameObject SkillObj => skillObj;
    public float SkillDuration => skillDuration;
    public int SkillEffectAmount => skillEffectAmount;

    public float SkillPushAmount => skillPushAmount;

    public float PerPushDurationAndStunTime => perPushDurationAndStunTime;

    public SpellType SpellType => spellType;
    public int Energy => energy;
}
