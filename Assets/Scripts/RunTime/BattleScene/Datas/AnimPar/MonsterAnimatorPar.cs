using UnityEngine;

[CreateAssetMenu]
public class MonsterAnimatorPar :AnimatorParemeterSet
{
    public int Chase_Hash => Animator.StringToHash(chase);

    public string Chase => chase; 

    public readonly string chaseAnimClipName = "Chase";
    [SerializeField] string chase;
}
