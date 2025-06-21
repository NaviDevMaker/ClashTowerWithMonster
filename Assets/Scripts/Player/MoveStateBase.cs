using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using System;



namespace Game.Players
{
    public class MoveStateBase<T> : StateMachineBase<T> where T : PlayerControllerBase<T>
    {

        public MoveStateBase(T controller) : base(controller) { }
        public bool isMoving = false;//true
        public bool isPressedA = false;
        int moveSpeed = 0;
        Vector3 offset = new Vector3(0, 0.3f, 0);
        SemaphoreSlim semaphore = new SemaphoreSlim(1,1);

        CancellationTokenSource cls = null;
        public override void OnEnter()
        {
            moveSpeed = controller.BuffStatus(BuffType.Speed, (int)controller.PlayerStatus.MoveSpeed);
            cls = controller.cls;
            controller.cls = new CancellationTokenSource();
            nextState = controller.IdleState;
            //Move().Forget();
            if(controller.previousState == controller.AttackState || controller.previousState == controller.IdleState)
            {
                isMoving = true;
                Move().Forget();
            }
        }
        public override  void OnUpdate()
        {
            moveSpeed = controller.BuffStatus(BuffType.Speed, (int)controller.PlayerStatus.MoveSpeed);
            var isBuffed = controller.statusCondition.BuffSpeed.isActive;
            if (isBuffed) { var newSpeed = 1.3f; controller.animator.speed = newSpeed;}
            if (!CheckMovable())
            {
                cls?.Cancel();
                isMoving = false;
            }
            Debug.Log($"animation.spped{controller.animator.speed}");
            if (InputManager.IsClickedMoveAndAutoAttack()) //Input.GetMouseButtonUp(0) && Input.GetKey(KeyCode.A)
            {
                controller.cls.Cancel();
                controller.cls.Dispose();
                controller.cls = new CancellationTokenSource();
                isPressedA = true;
                isMoving = true;
                Move().Forget();
            }
            else if(InputManager.IsCllikedMoveButton())
            {
                Debug.Log("方向変更");
                controller.cls.Cancel();
                controller.cls.Dispose();
                controller.cls = new CancellationTokenSource();
                isPressedA = false;
                isMoving = true;
                Move().Forget();
            }
            if (!isMoving) controller.ChangeState(nextState);
        }
        public override void OnExit()
        {
            controller.animator.SetBool(controller.AnimatorPar.Move, false);
            isPressedA = false;
        }

        async UniTask Move()
        {
            if (!controller.animator.GetBool(controller.AnimatorPar.Move)) controller.animator.SetBool(controller.AnimatorPar.Move, true);
            Debug.Log("呼ばれてるた,PlayerのMoveが");
            controller.cls = new CancellationTokenSource();
            cls = controller.cls;
            try
            {
                await semaphore.WaitAsync(cancellationToken: cls.Token);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hits = Physics.RaycastAll(ray);
                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        var hitLayer = 1 << hit.collider.gameObject.layer;
                        if (Layers.groundLayer == hitLayer)
                        {
                            Debug.Log("ヒット");
                            var targetPos = hit.point;
                            var direction = targetPos - controller.transform.position;
                            if (direction != Vector3.zero) controller.transform.rotation = Quaternion.LookRotation(direction);
                            if (!CheckMovable()) break;
                            //while ((targetPos - controller.transform.position).sqrMagnitude > 0.01f && !cls.IsCancellationRequested)
                            //{
                            //    if(cls.IsCancellationRequested)
                            //    {
                            //        break;
                            //    }
                            //    var move = Vector3.MoveTowards(controller.transform.position, targetPos,moveSpeed * Time.deltaTime);
                            //    if (!CheckMovable(move)) break;
                            //    controller.transform.position = move;
                            //    await UniTask.Yield();
                            //}

                            //await UniTask.Yield();
                            var moveTween = controller.transform.DOMove(targetPos, Vector3.Distance(controller.transform.position, targetPos) / moveSpeed)
                                .SetEase(Ease.Linear);
                            var task = moveTween.ToUniTask(cancellationToken: cls.Token);
                            //while (!cls.IsCancellationRequested && !task.Status.IsCompleted())
                            //{
                            //    Debug.Log($"確認中だよ＝＝＝＝");
                            //    if (!CheckMovable()) return;
                            //    await UniTask.Yield();
                            //}
                            //if (cls.IsCancellationRequested)
                            //{
                            //    Debug.Log("tweenがkillされました");
                            //    moveTween.Kill();
                            //}
                            await task;
                            Debug.Log(cls.IsCancellationRequested);
                        }
                    }
                }
            }
            finally
            {
                Debug.Log("おーーーーーーーいおわったよーーーーーーーーー");
                semaphore.Release();
                if (!cls.IsCancellationRequested) isMoving = false;
            }
        }
        bool CheckMovable()
        {
            var rayDistance = controller.PlayerStatus.AttackRange;
            if (Physics.Raycast(controller.transform.position + offset, controller.transform.forward, rayDistance, Layers.buildingLayer))
            {
                Debug.Log("通れません");
                return false;
            }

            return true;
        }
    }         
}

