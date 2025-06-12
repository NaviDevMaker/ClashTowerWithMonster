using UnityEngine;

public class AnimatorParemeterSet : ScriptableObject
{

    public int Attack { get => Animator.StringToHash(attack); } 
    public int Death { get => Animator.StringToHash(death); }

    public readonly string attackAnimClipName = "Attack";
    public readonly string deathAnimClipName = "Death";
    //public readonly int chase = Animator.StringToHash("isChasing");
    //public readonly int attack = Animator.StringToHash("isAttacking");
    //public readonly int death = Animator.StringToHash("isDead");
    [SerializeField] string attack;
    [SerializeField] string death;
}
