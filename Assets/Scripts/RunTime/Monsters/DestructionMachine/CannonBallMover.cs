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
                Debug.Log(target);
                var currentTarget = target;
                DamageToEnemy(currentTarget);
                EffectManager.Instance.hitEffect.GenerateHitEffect(currentTarget);
                OnEndProcess?.Invoke(this);
            }
        }

        public override void Move() => base.Move();
    }

}

