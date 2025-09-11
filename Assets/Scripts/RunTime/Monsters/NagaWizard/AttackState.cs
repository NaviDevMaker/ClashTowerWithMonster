using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.DualShock.LowLevel;

namespace Game.Monsters.NagaWizard
{
    public class AttackState : AttackStateBase<NagaWizardController>
    {
        public AttackState(NagaWizardController controller) : base(controller)
        {
            SetPrefabObject();
        }
        GameObject skeletonHead;
        float effectGenerateInterval = 0.125f;
        List<GameObject> currentSkeletonHeads = new List<GameObject>();
        GameObject nagaWizardPrefab;
        public override void OnEnter()
        {
            base.OnEnter();
            if (attackEndNomTime == 0f) StateFieldSetter.AttackStateFieldSet<NagaWizardController>(controller, this, clipLength,17,
                controller.MonsterStatus.AttackInterval);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        async void SetPrefabObject()
        {
            if (skeletonHead != null && nagaWizardPrefab != null) return;
            skeletonHead = await SetFieldFromAssets.SetField<GameObject>("Monsters/SpecialEffectMonsters/NagaWizardEffect_Head");
            nagaWizardPrefab = await SetFieldFromAssets.SetField<GameObject>("Monsters/SpecialEffectMonsters/NagaWizard");
        }

        protected override async UniTask Attack_Generic(SimpleAttackArguments attackArguments)
        {
            var arguments = new SimpleAttackArguments
            {
                getTargets = attackArguments.getTargets,
                attackEffectAction = GenerateSkeletonHeadEffect,
                attackEndAction = SkeletonHeadDisactive,
                repeatCount = controller.ContinuousAttackMonsterStatus._ContinuousAttackInfo.ContinuousCount
            };
            await base.Attack_Generic(arguments);
        }
        async void GenerateSkeletonHeadEffect()
        {
            var doubleCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token,controller.GetCancellationTokenOnDestroy());
            try
            {
                while(CanActionSkeletonHead(doubleCts))
                {
                    HeadGenerate(doubleCts);
                    await UniTask.Delay(TimeSpan.FromSeconds(effectGenerateInterval), cancellationToken: doubleCts.Token);
                }
            }
            catch (OperationCanceledException) { }
        }

        protected override void AddDamageToTarget(UnitBase currentTarget)
        {
            base.AddDamageToTarget(currentTarget);
            if(currentTarget.isDead)
            {
                var pos = currentTarget.transform.position;
             
                var flatPos = PositionGetter.GetFlatPos(pos);
                var myPos = PositionGetter.GetFlatPos(controller.transform.position);
                var direction = (myPos - flatPos).normalized;
                var rot = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f,0f,0f);// 
                flatPos.y = Terrain.activeTerrain.SampleHeight(flatPos);
                var collider = currentTarget.GetComponent<Collider>();
                SummmonNagaWizard(collider,flatPos,rot);
            }
        }
        async void SummmonNagaWizard(Collider collider,Vector3 flatPos,Quaternion rot)
        {
            var nagaWizard = UnityEngine.Object.Instantiate(nagaWizardPrefab, flatPos, rot);
            controller.SetSummonParticle(flatPos);
            var cmp = nagaWizard.GetComponent<NagaWizardController>();
            if (cmp != null)
            {
                cmp.isSummoned = true;
                cmp.ownerID = controller.ownerID;
            }
            var height = collider.bounds.size.y;
            var offsetY = 4.0f;
            var startPos = flatPos;
            startPos.y = height;
            var targetPos = startPos + Vector3.up * offsetY;
            var skeletonHeadObj = UnityEngine.Object.Instantiate(skeletonHead, startPos,rot);
            var mat = skeletonHeadObj.GetComponent<SkinnedMeshRenderer>().material;
            var color = mat.color;
            color.a = 1.0f;
            mat.color = color;
            var targetScale = skeletonHeadObj.transform.localScale * 5.0f;
            var duration = 2f;
            var moveSet = new Vector3TweenSetup(targetPos,duration);
            var scaleSet = new Vector3TweenSetup(targetScale, duration);
            var moveTask = skeletonHeadObj.Mover(moveSet).ToUniTask();
            var scaleTask = skeletonHeadObj.Scaler(scaleSet).ToUniTask();
            var fadeTask = DOTween.To(
                () => mat.color,
                x => mat.color = x,
                new Color(color.r, color.g,color.b, 0f),
                duration
            ).SetEase(Ease.Linear).ToUniTask();
            await UniTask.WhenAll(moveTask,fadeTask,scaleTask);
            UnityEngine.Object.Destroy(skeletonHeadObj);
        }
        async void SkeletonHeadDisactive()
        {
           currentSkeletonHeads.ForEach(s =>
           {
               if (s == null) return;
               AlphaChange(s,isEnactive:false);
           });

            var particles = currentSkeletonHeads.Select(s =>
            {
                return s.GetComponentsInChildren<ParticleSystem>().ToList();
            })
            .SelectMany(p => p).ToList();
            var tasks = particles.Select(p =>
            {
                var main = p.main;
                main.loop = false;
                main.simulationSpeed *= 2.5f;
                return RelatedToParticleProcessHelper.WaitUntilParticleDisappear(p);
            }).ToArray();
            await UniTask.WhenAll(tasks);
            particles.ForEach(p =>
            {
                var main = p.main;
                main.simulationSpeed = 1.0f;
            });
            currentSkeletonHeads.ForEach(s => s.gameObject.SetActive(false));
            currentSkeletonHeads.Clear();
            currentSkeletonHeads.TrimExcess();
        }
        async void HeadGenerate(CancellationTokenSource doubleCts)
        {
            var sketon = PoolObjectPreserver.SkeletonHeadGetter();
            var pos = controller.EffectEmit.position;
            var rot = controller.transform.rotation;
            if(sketon == null)
            {
                var sketonObj = UnityEngine.Object.Instantiate(skeletonHead);
                sketon = sketonObj;
                PoolObjectPreserver.skeletonHeadObjList.Add(sketonObj);
            }
            sketon.gameObject.SetActive(true);
            AlphaChange(sketon, isEnactive: true);
            sketon.transform.position = pos;
            sketon.transform.rotation = rot * Quaternion.Euler(-90f,0f,0f);
            currentSkeletonHeads.Add(sketon);
            sketon.GetComponentsInChildren<ParticleSystem>().ToList().ForEach(p =>
            {
                Debug.Log("ナガのエフェクトを再生します");
                var main = p.main;
                main.loop = true;
                p.Play();
            });

            try
            {
                RotateSkeletonHead(sketon, doubleCts);
                var moveSpeed = 10f;
                var body = target.BodyMesh;
                var targetPos = body.bounds.center;
                var currentPos = sketon.transform.position;

                Func<GameObject, float, UniTask> moveAction = async (moveObj, distance) =>
                {
                    try
                    {
                        while (CanActionSkeletonHead(doubleCts) && (Vector3.Distance(currentPos, targetPos) > distance))
                        {
                            targetPos = body.bounds.center;
                            var move = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
                            moveObj.transform.position = move;
                            currentPos = move;
                            await UniTask.Yield(cancellationToken: doubleCts.Token);
                        }
                    }
                    catch (OperationCanceledException) {throw;}           
                };
                await moveAction(sketon,3.0f);
                AlphaChange(sketon,isEnactive: false);
                var particle = sketon.GetComponentInChildren<ParticleSystem>();
                currentPos = particle.gameObject.transform.position;
                await moveAction(particle.gameObject, Mathf.Epsilon);
                var main = particle.main;
                main.simulationSpeed *= 2.5f;
                main.loop = false;
                await RelatedToParticleProcessHelper.WaitUntilParticleDisappear(particle);
            }
            catch (OperationCanceledException) { }
        }
        async void RotateSkeletonHead(GameObject headObj,CancellationTokenSource doubleCts)
        {
            var rotateSpeed = 3f;
            try
            {
                while (CanActionSkeletonHead(doubleCts))
                {
                    headObj.transform.Rotate(Vector3.up * 360f * rotateSpeed * Time.deltaTime);
                    await UniTask.Yield(cancellationToken: doubleCts.Token);
                }
            }
            catch (OperationCanceledException) { }        
        }
        void AlphaChange(GameObject skeleton,bool isEnactive)
        {
            var renderer = skeleton.GetComponent<SkinnedMeshRenderer>();
            var mat = renderer.material;
            var alpha = isEnactive ? 107f / 255f : 0f;
            var originalColor = mat.color;
            originalColor.a = alpha;
            mat.color = originalColor;
        }
        bool CanActionSkeletonHead(CancellationTokenSource doubleCts)
        {
            var isCanceled = doubleCts.IsCancellationRequested;
            var isDead = target.isDead;
            var isFreezed = controller.statusCondition.Freeze.isActive;
            return !isCanceled && !isFreezed && !isDead && !isInterval;
        }
    }
}