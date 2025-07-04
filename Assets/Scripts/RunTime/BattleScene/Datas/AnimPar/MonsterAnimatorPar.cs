using UnityEngine;

[CreateAssetMenu]
public class MonsterAnimatorPar :AnimatorParemeterSet
{
    public int Chase => Animator.StringToHash(chase);
    public readonly string chaseAnimClipName = "Chase";
    [SerializeField] string chase;
}
