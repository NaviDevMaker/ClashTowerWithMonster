using UnityEngine;

public interface IEffectSetter
{
    void SetEffect();
}

public class StatusConditionEffect
{
    public ParesisEffect paresisEffect { get;private set; }
    public BuffEffect buffEffect { get; private set; }

    public ToxicSmokeEffect toxicSmokeEffect { get; private set; }

    public ConfusionEffect confusionEffect { get; private set; }
    public StatusConditionEffect()
    {
        paresisEffect = new ParesisEffect();
        buffEffect = new BuffEffect();
        toxicSmokeEffect = new ToxicSmokeEffect();
        confusionEffect = new ConfusionEffect();
    }
}
public  class EffectManager : SingletonMonobehavier<EffectManager>
{
   public StatusConditionEffect statusConditionEffect { get; private set;}
   public MagicCircleEffect magicCircleEffect { get; private set;}
   public DeathEffect deathEffect { get; private set;}
   public HitEffect hitEffect { get; private set;}
   public HealEffect healEffect { get; private set; }
   public ExplosionEffect expsionEffect { get; private set;}
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
        expsionEffect = new ExplosionEffect();
        healEffect = new HealEffect();
    }
}
