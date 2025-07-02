using UnityEngine;

[CreateAssetMenu]
public class PlayerAnimatorPar : AnimatorParemeterSet
{
    public  int Move => Animator.StringToHash(move);
    public int Idle => Animator.StringToHash(idle);
    public readonly string moveAnimClipName = "Move";
    [SerializeField] string move;
    [SerializeField] string idle;
}
