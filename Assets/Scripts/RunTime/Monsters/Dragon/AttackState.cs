using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Monsters.Dragon
{
    public class AttackState : AttackStateBase<DragonController>
    {
        public AttackState(DragonController controller) : base(controller) { }
        ParticleSystem projectileFireEffect;
        class HitJudgeColliderInfo
        {
           public GameObject hitObj = null;
           public float absRightAmount = 2.0f;
           public float zOffset = 5.0f;
           public Vector3 GetStartPos(DragonController controller)
           {
               var offset_hitJudge = controller.transform.right * absRightAmount + controller.transform.forward * zOffset;
               var pos = controller.transform.position + offset_hitJudge;
               var flatPos = PositionGetter.GetFlatPos(pos);
               return flatPos;
           }
        }

        HitJudgeColliderInfo hitJudgeColliderInfo;
        CancellationTokenSource doubleCls = null;
        bool isBreathFire = false;
        public override void OnEnter()
        {
            hitJudgeColliderInfo = new HitJudgeColliderInfo();
            SetProjectileFireEffect();
            base.OnEnter();
            //This paremetars are examples,so please change it to your preference!!
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<DragonController>(controller, this, clipLength,28,
                controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
            isBreathFire = false;
        }
        protected override async UniTask Attack_Generic(AttackArguments attackArguments)
        {
            if (!controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName))
            {
                controller.animator.Play(controller.MonsterAnimPar.attackAnimClipName);
            }
            float startNormalizeTime = 0f;
            float now = 0f;
            try
            {
                if (!controller.statusCondition.Freeze.isActive) LookToTarget();
                await UniTask.WaitUntil(() =>
                {
                    if (controller.isDead) return false;
                    return controller.animator.GetCurrentAnimatorStateInfo(0).IsName(controller.MonsterAnimPar.attackAnimClipName);
                }, cancellationToken: cts.Token);
                controller.animator.speed = 1.0f;
                Debug.Log(target.gameObject.name);
                startNormalizeTime = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                Func<bool> wait = (() =>
                {
                    if (controller.isDead) return true;
                    now = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    return now - startNormalizeTime >= attackEndNomTime;
                });

                await UniTask.WaitUntil(wait, cancellationToken: cts.Token);
             
                Func<float> getCurrentNorm = () =>
                {
                   var current = controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                   return current - startNormalizeTime;
                };
                PlayProjectileFireEffect(getCurrentNorm,attackArguments.getTargets);
            }
            catch (OperationCanceledException)
            {
                var elapsedTime = (now - startNormalizeTime) * clipLength;
                leftLengthTime = Mathf.Max(0f, clipLength - elapsedTime / stateAnimSpeed);
                isAttacking = false;
            }
            catch(ObjectDisposedException){ }
            finally{}
            leftLengthTime = 0f;
        }

        protected override bool CheckAttackable()
        {
            Debug.Log(target.gameObject.name);
            var canAttack = false;
            var targetPos = Vector3.zero;
            var isDead = target.isDead;
            var collider = target.GetComponent<Collider>();
            var closestPos = collider.ClosestPoint(controller.transform.position);

            targetPos = PositionGetter.GetFlatPos(closestPos);
            var myPos = PositionGetter.GetFlatPos(controller.transform.position);
            var isConfused = controller.statusCondition.Confusion.isActive;
            var targetSide = target.GetUnitSide(controller.ownerID);
            var effectiveSide = isConfused switch
            {
                true => Side.PlayerSide | Side.EnemySide,
                false => Side.EnemySide,
            };

            return canAttack =Å@!isBreathFire ? (targetPos - myPos).magnitude <= attackRange && !isDead && (targetSide & effectiveSide) != 0
                 : !isDead && (targetSide & effectiveSide) != 0;// && !isDead;         
        }
        async void PlayProjectileFireEffect(Func<float> getCurrentNorm,Func<List<UnitBase>> getCurrentTargets)
        {
            Debug.Log("âäÇìfÇ´Ç‹Ç∑");
            var fire = default(ParticleSystem);
            try
            {
                isBreathFire = true;
                doubleCls = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, controller.GetCancellationTokenOnDestroy());
                var rotX = Quaternion.Euler(45f, 0f, 0f);
                var targetMonstertRot = controller.transform.rotation * rotX;
                await RotateMonster(doubleCls,targetMonstertRot);

              Debug.Log("âÒì]èIÇÌÇË");
              controller.transform.rotation = targetMonstertRot;
              var offset = new Vector3(0f, 1.0f, 1.0f);
              var pos = controller.transform.position + offset;
              fire = UnityEngine.Object.Instantiate(projectileFireEffect,pos,Quaternion.identity);
              fire.transform.SetParent(controller.transform);
              fire.transform.localRotation = Quaternion.identity;
              fire.transform.localRotation *= Quaternion.Euler(-90f, 0f, 20f);
              if (controller is IRangeAttack rangeAttack && hitJudgeColliderInfo.hitObj == null)
              {
                 var _hitObj = controller.FlyProjectileStatusData.ProjectileHitJudgeObj;
                 if (_hitObj != null)
                 {
                     hitJudgeColliderInfo.hitObj = UnityEngine.Object.Instantiate(_hitObj, pos, Quaternion.identity); 
                     rangeAttack.rangeAttackObj = hitJudgeColliderInfo.hitObj;
                 }
                 Debug.Log(rangeAttack.rangeAttackObj);
              }
              var colliderPos = hitJudgeColliderInfo.GetStartPos(controller);
              var colObj = hitJudgeColliderInfo.hitObj;
              colObj.SetActive(true);
              colObj.transform.position = colliderPos;
              var euler = fire.transform.eulerAngles;
              var startEulerZ = euler.z;
              var targetEulerZ = -startEulerZ;
              
              var startColPos = colObj.transform.position;
              var targetPos = colObj.transform.position - controller.transform.right * hitJudgeColliderInfo.absRightAmount;
              var startLength = getCurrentNorm() * clipLength;
              var leftLength = clipLength - startLength;
              var interval =  leftLength / (float)controller.repeatCount;
              Debug.Log($"ÉCÉìÉ^Å[ÉoÉãÇÕ{interval}");
              var particles = new List<ParticleSystem> {fire};
              particles.AddRange(fire.GetComponentsInChildren<ParticleSystem>());
              particles.ForEach(p =>
              {     
                 var main = p.main;
                 main.loop = true;
                 p.Play();
              });
              if (target == null) return;

              UnityAction damageEachUnit = async () =>
              {
                  try
                  {
                      while (CanFireShot(doubleCls))
                      {
                          var currentTargets = getCurrentTargets();
                          currentTargets.ForEach(target =>
                          {
                              AddDamageToTarget(target);
                          });
                          await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: controller.GetCancellationTokenOnDestroy());
                      }
                  }
                  catch (OperationCanceledException) { }
                 
              };
              damageEachUnit.Invoke();
              while (CanFireShot(doubleCls))
              {
                  var elapsedTime = clipLength * getCurrentNorm() - startLength;
                  var lerp = Mathf.Clamp01(elapsedTime / leftLength);
                  var targetZ = Mathf.Lerp(startEulerZ, targetEulerZ, lerp);
                  var targetRot = Quaternion.Euler(euler.x, euler.y, targetZ);
                  var colTargetPos = Vector3.Lerp(startColPos,targetPos,lerp);
                  fire.transform.rotation = targetRot;
                  colObj.transform.position = colTargetPos;
                  await UniTask.Yield(cancellationToken:doubleCls.Token);
              }
            }
            catch (OperationCanceledException) {}
            catch (ObjectDisposedException) { }
            finally
            {
                var rotX = Quaternion.Euler(-45f, 0f, 0f);
                var targetMonstertRot = controller.transform.rotation * rotX;
                hitJudgeColliderInfo.hitObj.SetActive(false);
                //RotateMonster(doubleCls,targetMonstertRot,isEnd:true).Forget();
                if (fire != null)
                {
                    fire.transform.SetParent(null);
                    DestroyParticles(fire);
                };
                isBreathFire = false;
            }
        }
        async void SetProjectileFireEffect()
        {
            if (projectileFireEffect != null) return;
            var fireObj = await SetFieldFromAssets.SetField<GameObject>("Effects/DragonFireEffect");
            if (fireObj == null) return;
            projectileFireEffect = fireObj.GetComponent<ParticleSystem>();
        }

        async UniTask RotateMonster(CancellationTokenSource doubleCls,Quaternion targetRot,bool isEnd = false)
        {
            try
            {
                var rotateSpeed = 200f;
                while (Quaternion.Angle(controller.transform.rotation, targetRot) > 0.1 && CanFireShot(doubleCls, isEnd))
                {
                    Debug.Log("âÒì]íÜÇ≈Ç∑");
                    controller.transform.rotation = Quaternion.RotateTowards(controller.transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                    await UniTask.Yield(cancellationToken: doubleCls.Token);
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
        }
        bool CanFireShot(CancellationTokenSource doubleCls,bool isEnd = false)
        {
            try
            {
                var cancelled = doubleCls.IsCancellationRequested;
                var isFreezed = controller.statusCondition.Freeze.isActive;
                var isDead = target.isDead;
                if (isEnd) return !cancelled && !isFreezed && !isDead;
                else return !cancelled && !isFreezed && !isDead && !isInterval;
            }
            catch (ObjectDisposedException) { return false; }
           
        }
        async void DestroyParticles(ParticleSystem fireParticle)
        {
            var tasks = Enumerable.Empty<UniTask>().ToList();
            var particles = fireParticle.GetComponentsInChildren<ParticleSystem>().ToList();
            particles.Add(fireParticle);
            particles.ForEach(p =>
            {
                if (p == null) return;
                var main = p.main;
                main.loop = false;
                main.simulationSpeed *= 7f;
                var task = RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
                tasks.Add(task);
            });
            await tasks;
            particles.ForEach(p =>
            {
                if (p == null) return;
                UnityEngine.Object.Destroy(p.gameObject);
            });
        }
    }
}