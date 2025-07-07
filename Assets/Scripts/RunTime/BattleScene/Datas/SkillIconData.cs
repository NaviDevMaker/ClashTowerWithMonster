using UnityEngine;

[CreateAssetMenu]
public class SkillIconData : ScriptableObject
{
    [SerializeField] Sprite iconImage;
    [SerializeField] Sprite energyImage;
    [SerializeField] int energy;

    public Sprite IconImage => iconImage;
    public Sprite EnergyImage => energyImage;
    public int Energy => energy; 
}
