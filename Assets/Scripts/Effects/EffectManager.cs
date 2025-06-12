using UnityEngine;

public  class EffectManager : SingletonMonobehavier<EffectManager>
{
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
    }
}
