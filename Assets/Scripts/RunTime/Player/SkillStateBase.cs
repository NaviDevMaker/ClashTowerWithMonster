using Cysharp.Threading.Tasks;
using Game.Players;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillStateBase<T> : StateMachineBase<T> where T : PlayerControllerBase<T>
{
    public SkillStateBase(T controller) : base(controller) { }

    protected PlayerSpellEffectHelper spellEffectHelper = null;
    protected HashSet<ParticleSystem> particles = new HashSet<ParticleSystem>();
    protected GameObject particleObject { get; private set; } = null;
    protected float timerOffsetY { get; private set; }
    protected float duration = 0f;
    public override void OnEnter() { }
    public async void SkillInvoke()
    {
        controller.isUsingSkill = true;
        controller.animator.SetBool(controller.AnimatorPar.Skill, controller.isUsingSkill);
        Debug.Log("スキル発動！！！！");
        SetUp();
        Spell();
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
        controller.isUsingSkill = false;
        controller.animator.SetBool(controller.AnimatorPar.Skill, controller.isUsingSkill);      
    }
    public override void OnExit() { }
    public override void OnUpdate() { }

    protected virtual void SetUp()
    {
        if (spellEffectHelper == null)
        {
            duration = controller.SkillData.SkillDuration;
            var bodyHeight = controller.BodyMesh.bounds.size.y;
            bodyHeight += 2.0f;
            timerOffsetY = bodyHeight;
            var amount = controller.SkillData.SkillEffectAmount;
            var type = controller.SkillData.SpellType;
            var spellObj = controller.SkillData.SkillObj;
            particleObject = UnityEngine.Object.Instantiate(spellObj);
            spellEffectHelper = new PlayerSpellEffectHelper(amount, type, particleObject,controller.ownerID);
        }
        if (particles.Count == 0) SetParticles();
    }
    protected virtual void SetParticles() { }
    protected virtual async UniTask EnactiveAll() => await UniTask.CompletedTask;
    protected virtual async void Spell() => await UniTask.CompletedTask;
}
