using UnityEngine;

public class StatusConditionEffect
{
    public ParesisEffect paresisEffect { get; set; }
    public StatusConditionEffect()
    {
        paresisEffect = new ParesisEffect();
    }
}
public  class EffectManager : SingletonMonobehavier<EffectManager>
{
   

   public StatusConditionEffect statusConditionEffect { get; private set;}
   public MagicCircleEffect magicCircleEffect { get; private set;}
   public DeathEffect deathEffect { get; private set;}
   public HitEffect hitEffect { get; private set;}


    private void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        magicCircleEffect = new MagicCircleEffect();
        deathEffect = new DeathEffect();
        hitEffect = new HitEffect();
        statusConditionEffect = new StatusConditionEffect();
    }
}
