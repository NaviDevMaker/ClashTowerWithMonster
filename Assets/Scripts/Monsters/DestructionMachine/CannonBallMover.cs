using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;

namespace Game.Monsters.DestructionMachine
{
    public class CannonBallMover : LongDistanceAttack<DestructionMachineController>
    {
        protected override void Update()
        {
            base.Update();
            if (IsReachedTargetPos && attacker.AttackState.target != null) 
            {
                DamageToEnemy(target);
                EffectManager.Instance.hitEffect.GenerateHitEffect(target).Forget();
                OnEndProcess?.Invoke(this);
            }
        }

        public override void Move() => base.Move();
  
        protected override IEnumerator MoveToEnemy()
        {
            Debug.Log("Œü‚©‚¢‚Ü‚·");
            var height = TargetPositionGetter.GetTargetHeight(target);

            var targetPos = target.transform.position + new Vector3(0f, height, 0f);
            while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f
                && (!target.isDead && target != null))
            {
                targetPos = target.transform.position + new Vector3(0f, height, 0f);
                var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                var direction = targetPos - transform.position;
                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = rotation;
                }
                transform.position = move;
                yield return null;
            }

            if (target.isDead && target == null)
            {
                targetPos.y = Terrain.activeTerrain.SampleHeight(targetPos);
                while ((targetPos - transform.position).magnitude > Mathf.Epsilon + 0.1f)
                {
                    var move = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
                    var direction = targetPos - transform.position;
                    if (direction != Vector3.zero)
                    {
                        Quaternion rotation = Quaternion.LookRotation(direction);
                        transform.rotation = rotation;
                    }
                    transform.position = move;
                    yield return null;
                }

            }

            transform.position = targetPos;
            moveCoroutine = null;
            IsReachedTargetPos = true;
        }
    }

}

