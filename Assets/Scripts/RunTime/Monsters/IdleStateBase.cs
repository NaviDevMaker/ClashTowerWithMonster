using Game.Monsters.Slime;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using DG.Tweening;
namespace Game.Monsters
{
    public class IdleStateBase<T> : StateMachineBase<T> where T : MonsterControllerBase<T>
    {
        public IdleStateBase(T controler) : base(controler) { }
        public bool isEndSummon = false;
        public override void OnEnter() { }
        public override void OnUpdate()
        {
            if (isEndSummon) controller.ChangeState(nextState);
        }
        public override void OnExit() { }

        //idleの処理のみ同じだからこのメソッドの後に親クラスでは召喚時効果などを呼び出す
        protected virtual async UniTask OnEnterProcess()
        {
            try
            {
                var summonWaitTime = controller.MonsterStatus.SummonWaitTime;
                Func<bool> isSummoned = (() => controller.isSummoned);
                await UniTask.WaitUntil(isSummoned);
                AllResetBoolProparty();
                nextState = controller.ChaseState;
                UIManager.Instance.StartSummonTimer(summonWaitTime, controller).Forget();
                await UniTask.Yield();
                SummonMoveAction();
                await UniTask.Delay(TimeSpan.FromSeconds(summonWaitTime));
                isEndSummon = true;
            }
            catch(OperationCanceledException) {}
        }

        protected virtual void AllResetBoolProparty()
        {
            controller.animator.SetBool(controller.MonsterAnimPar.Chase_Hash, false);
            controller.animator.SetBool(controller.MonsterAnimPar.Attack_Hash, false);
        }
        async void SummonMoveAction()
        {
            try
            {
                Debug.Log("落下します");
                var offsetY = 8.0f;
                var targetPos = controller.transform.position;
                var appearPos = targetPos + Vector3.up * offsetY;
                controller.transform.position = appearPos;
                var moveSpeed = 30.0f;
                var originalScale = controller.transform.localScale;
                var scale = new Vector3(originalScale.x * 0.3f, originalScale.y * 3f, originalScale.y * 0.3f);
                controller.transform.localScale = scale;
                while ((controller.transform.position - targetPos).magnitude > 0.1f)
                {
                    var currentPos = controller.transform.position;
                    var move = Vector3.MoveTowards(currentPos, targetPos, Time.deltaTime * moveSpeed);
                    controller.transform.position = move;

                    if (controller.transform.position.y <= targetPos.y)
                    {
                        controller.transform.position = targetPos;
                        break;
                    }
                    await UniTask.Yield(cancellationToken: controller.GetCancellationTokenOnDestroy());
                }

                var sequence = DOTween.Sequence();
                var targetScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
                var duration = 0.2f;
                sequence.Append(controller.transform.DOScale(targetScale, duration))
                    .Append(controller.transform.DOScale(originalScale, duration));
            }
            catch (OperationCanceledException) { throw; }
        }
    }

}

