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
        SemaphoreSlim semaphore = new SemaphoreSlim(1,1);

        CancellationTokenSource cls = null;
        public override void OnEnter()
        {
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
        public override void OnUpdate()
        {
            if(!CheckMovable()) return;
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
            if(!controller.animator.GetBool(controller.AnimatorPar.Move)) controller.animator.SetBool(controller.AnimatorPar.Move, true);

            controller.cls = new CancellationTokenSource();
            cls = controller.cls;
            try
            {
                await semaphore.WaitAsync(cancellationToken:cls.Token);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var hitLayer = 1 << hit.collider.gameObject.layer;
                    if (Layers.groundLayer == hitLayer)
                    {
                        Debug.Log("ƒqƒbƒg");
                        var targetPos = hit.point;
                        var direction = targetPos - controller.transform.position;
                        if (direction != Vector3.zero) controller.transform.rotation = Quaternion.LookRotation(direction);
                        await controller.transform.DOMove(targetPos, Vector3.Distance(controller.transform.position, targetPos) / controller.PlayerStatus.MoveSpeed)
                            .SetEase(Ease.Linear).ToUniTask(cancellationToken: cls.Token);
                        //transform.position = targetPos;
                    }
                }
            }
            finally
            {
                semaphore.Release();
                if(!cls.IsCancellationRequested) isMoving = false;
            }
        }  
        
        bool CheckMovable()
        {
            var rayDistance = controller.PlayerStatus.AttackRange;
            if(Physics.Raycast(controller.transform.position,controller.transform.forward,rayDistance,Layers.buildingLayer))
            {
                Debug.Log("’Ê‚ê‚Ü‚¹‚ñ");
                cls?.Cancel();
                isMoving = false;
                controller.ChangeState(nextState);
                return false;
            }

            return true;
        }
    }         
}

