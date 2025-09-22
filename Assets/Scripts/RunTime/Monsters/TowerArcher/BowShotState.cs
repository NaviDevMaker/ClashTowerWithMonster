
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Game.Monsters.Archer
{
    public class BowShotState : StateMachineBase<ArcherController>,IUnitAttack
    {
        readonly string stateName = "BowShot";
        float firstShotNomTime = 0.5f;
        int totalFrame = 0;
        int stanceFrame = 5;
        float time = 0f;
        bool isFirstShot = false;
        GunMover nextGun = null;
        public BowShotState(ArcherController controller) : base(controller) { }
   
        public override void OnEnter()
        {
            //controller.animator.SetBool(controller.shot,true);
            nextState = controller._IdleState;
            if (clipLength == 0)
            {
                clipLength = controller.GetAnimClipLength();
                var clip = controller.animator.GetAnimationClip(stateName);
                var frameRate = clip.frameRate;
                totalFrame = Mathf.RoundToInt(frameRate * clipLength);
                firstShotNomTime = stanceFrame / totalFrame;
            }
          
            var speed = clipLength / controller.shotDuration;
            controller.animator.speed = speed;
            Attack();
        }
        public override void OnUpdate()
        {
            if(time >= controller.shotDuration)
            {
                isFirstShot = false;
                time = 0f;
            }
            else time += Time.deltaTime;
            Debug.Log(controller.target);
            
            if (controller.target == null)
            {
                controller.ChangeState(nextState);
                return;
            }
            LookEnemy();
        }
        public override void OnExit()
        {
            controller.animator.SetBool(controller.shot,false);
            controller.animator.speed = 1.0f;
        }

        void LookEnemy()
        {
            if (controller.target == null) return;
            var direction = (controller.target.transform.position - controller.transform.position).normalized;
            direction.y = 0;

            if (direction == Vector3.zero) return;
            var rotation = Quaternion.LookRotation(direction);
            controller.transform.rotation = rotation;
        }

        public void Attack()
        {           
            Debug.Log("発射");
            if (!isFirstShot)
            {
                isFirstShot = true;
                Debug.Log("最初の一発");
                controller.animator.Play(stateName, 0, firstShotNomTime);
                controller.animator.SetBool(controller.shot, true);
            }
            else controller.animator.SetBool(controller.shot, true);

        }

        public void SetparentToNull()
        {
            Debug.Log("親を外します");
          if(nextGun != null) nextGun.transform.SetParent(null);
        }
        public void ActiveArrowFromAnimEvent()
        {

            if (nextGun != null)return;
            foreach (var gun in controller.shotGuns)
            {
                if (!gun.gameObject.activeSelf)
                {
                    nextGun = gun;
                    break;
                }
            }
            if (nextGun != null)
            {
                nextGun.gameObject.SetActive(true);
                Debug.Log("打たれました");

            }
        }
        public void ShotToEnemyFromAnimEvent()
        {
            if (nextGun == null) return;
            if(controller.target == null)
            {
                nextGun.OnEndProcess?.Invoke(nextGun);
                return;
            }
         
            nextGun.target = controller.target;
            nextGun.Move();
            Debug.Log("エラーの原因");
            nextGun = null;
        }
    }
}

