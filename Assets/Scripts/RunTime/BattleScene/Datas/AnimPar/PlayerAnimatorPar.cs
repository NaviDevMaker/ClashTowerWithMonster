using UnityEngine;

[CreateAssetMenu]
public class PlayerAnimatorPar : AnimatorParemeterSet
{
    public  int Move => Animator.StringToHash(move);
    public int Idle => Animator.StringToHash(idle);
    public int Skill => Animator.StringToHash(skill);

    public readonly string moveAnimClipName = "Move";
    [SerializeField] string move;
    [SerializeField] string idle;
    [SerializeField] string skill;
}
