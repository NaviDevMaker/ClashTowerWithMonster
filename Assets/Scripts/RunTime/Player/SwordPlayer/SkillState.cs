using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Players.Sword
{
    public class SkillState : SkillStateBase<SwordPlayerController>
    {
        public SkillState(SwordPlayerController controller) : base(controller) { }
        PushablePlayerSkillObj pushableObj = null;
        protected override void SetUp()
        {
            if(particles.Count != 0) foreach(var p in particles) { var main = p.main;main.loop = true; }
            base.SetUp();
            if (controller.addForceToUnit_Skill == null)
            {
                var data = controller.SkillData;
                var pushAmount = data.SkillPushAmount;
                var perDur = data.PerPushDurationAndStunTime;
                var pushEffect = PushEffectUnit.OnlyEnemyUnit;
                pushableObj = particleObject.GetComponent<SwordObj>();
                if(pushableObj != null) pushableObj.Initialize(controller.transform, timerOffsetY,controller.ownerID);
                controller.addForceToUnit_Skill = new AddForceToUnit<PushablePlayerSkillObj>(pushableObj,pushAmount,perDur, pushEffect);
            }
            particleObject.transform.SetParent(controller.transform);
            particleObject.transform.localPosition = Vector3.zero;
            var offsetY = 0.5f;
            particleObject.transform.localPosition += Vector3.up * offsetY;
        }
        protected override async void Spell()
        {
            UIManager.Instance.StartSkillTimer(duration,pushableObj);
            foreach (var p in particles)
            {
                p.gameObject.SetActive(true);
                p.Play();
            }
            var time = 0f;
            var damageInterval = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                damageInterval += Time.deltaTime;
                if(damageInterval >= 0.5f)
                {
                    damageInterval = 0f;
                    spellEffectHelper.EffectToUnit();
                    controller.addForceToUnit_Skill.KeepDistance(MoveType.Spell);
                }
                await UniTask.Yield();
            }
            foreach (var p in particles) { var main = p.main; main.loop = false; }
            await EnactiveAll();
            foreach (var p in particles) p.gameObject.SetActive(false);
        }
        protected override void SetParticles()
        {
            var childPars = particleObject.GetComponentsInChildren<ParticleSystem>().ToList();
         
            childPars.ForEach(p =>
            {
                var main = p.main;
                main.loop = true;
            });
            particles.AddRange(childPars);
            foreach (var p in particles) p.gameObject.SetActive(false);
        }
        protected override async UniTask EnactiveAll()
        {
            var tasks = GetEndParticleTasks();
            await UniTask.WhenAll(tasks);  
        }
        HashSet<UniTask> GetEndParticleTasks()
        {
            var tasks = new HashSet<UniTask>();
            foreach(var par in particles)
            {
                var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(par);
                tasks.Add(task);
            }
            return tasks;
        }
    }
}


