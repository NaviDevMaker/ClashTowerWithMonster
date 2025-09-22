using UnityEngine;

[CreateAssetMenu]
public class KingDragonAnimPar : AnimatorParemeterSet
{
    [SerializeField] string attack_2;
    public int Attack2_Hash => Animator.StringToHash(attack_2);
    public string attack_2AnimClipName => "Attack_2";
}
