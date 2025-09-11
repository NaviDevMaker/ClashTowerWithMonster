using Cysharp.Threading.Tasks;
using Game.Monsters.SpiderEgg;
using System;
using System.Threading;
using UnityEngine;

namespace Game.Monsters.Spider
{
    public class LayAnEggState : StateMachineBase<SpiderController>
    {
        public LayAnEggState(SpiderController controller) : base(controller) { }

        readonly int layAnEgg_Hash = Animator.StringToHash("isLayingEgg");
        bool isEndSpawn = false;
        public override void OnEnter()
        {
            try
            {
                controller.ChaseState.cts?.Cancel();
            }
            catch (ObjectDisposedException) { }
            controller.animator.SetTrigger(layAnEgg_Hash);
            nextState = controller.ChaseState;
            SpawnEgg();
        }
        public override void OnExit() { isEndSpawn = false; }
        public override void OnUpdate() 
        {
            if (isEndSpawn) controller.ChangeState(nextState);
        }
        async void SpawnEgg()
        {
            var layAnEggInfo = controller.layAnEggInfo;
            var targetScale = layAnEggInfo.eggPrefab.transform.lossyScale;
            var eggPrefab = layAnEggInfo.eggPrefab;
            var startPos = controller.SpawnEggTra.position;
            var startScale = Vector3.zero;
            var targetPos = startPos;
            targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);//
            //Debug.Log($"ターゲットは{targetPos}、スタートは{startPos}");
            var rot = eggPrefab.transform.rotation;
            var eggObj = UnityEngine.Object.Instantiate(eggPrefab,startPos, rot);
            eggObj.transform.localScale = startScale;
            try
            {
               
                await UniTask.WaitUntil(() => controller.animator.GetCurrentAnimatorStateInfo(0).IsName("LayAnEgg"),
                                              cancellationToken: controller.GetCancellationTokenOnDestroy());
                Func<float> currentNormalizedTime = () => controller.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                var cts = CancellationTokenSource.CreateLinkedTokenSource(controller.GetCancellationTokenOnDestroy());
                PlayHipSmoke();
                while (currentNormalizedTime() < 1.0f && !cts.IsCancellationRequested
                      && !controller.isDead)
                {
                    controller.hipSmoke.DamageInRangeUnit();
                    var lerp = currentNormalizedTime();
                    var currentScale = Vector3.Lerp(startScale, targetScale, lerp);
                    var currentPos = Vector3.Lerp(startPos, targetPos, lerp);
                    Debug.Log(currentPos);
                    eggObj.transform.position = currentPos;
                    eggObj.transform.localScale = currentScale;
                    await UniTask.Yield(cancellationToken: cts.Token);
                }
                if (controller.isDead) return;
                Debug.Log(targetPos);
                eggObj.transform.position = targetPos;
                eggObj.transform.localScale = targetScale;
                var egg = eggObj.GetComponent<SpiderEggController>();
                egg.ownerID = controller.ownerID;
                egg.isSummoned = true;
                isEndSpawn = true;
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (controller.isDead && eggObj != null) UnityEngine.Object.Destroy(eggObj); 
            }           
        }

        void PlayHipSmoke()
        {
            var smoke = controller.layAnEggInfo.hipSmoke;
            if (smoke != null)
            {
                smoke.Play();
            }
        }
    }
}

