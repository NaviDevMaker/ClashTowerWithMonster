using UnityEngine;

public class AnimatorParemeterSet : ScriptableObject
{

    public int Attack { get => Animator.StringToHash(attack); } 
    public int Death { get => Animator.StringToHash(death); }

    public readonly string attackAnimClipName = "Attack";
    public readonly string deathAnimClipName = "Death";
    
    [SerializeField] string attack;
    [SerializeField] string death;
}
