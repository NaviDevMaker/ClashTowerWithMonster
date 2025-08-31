using UnityEngine;

public class AnimatorParemeterSet : ScriptableObject
{

    public int Attack_Hash { get => Animator.StringToHash(attack); } 
    public int Death_Hash { get => Animator.StringToHash(death); }
    public string Attack => attack;
    public string Death => death;

    public readonly string attackAnimClipName = "Attack";
    public readonly string deathAnimClipName = "Death";
    
    [SerializeField] string attack;
    [SerializeField] string death;
}
