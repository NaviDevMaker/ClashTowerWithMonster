using System.Collections.Generic;
using UnityEngine;


public interface IRepeatAttack
{ 
    int repeatCount { get; }
}

namespace Game.Monsters.Dragon
{
    public class DragonController : MonsterControllerBase<DragonController>,IRangeAttack,IRepeatAttack
    {
        public GameObject rangeAttackObj { get; set; }

        public int repeatCount => 6;

        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }

        protected override void Update()
        {
            base.Update();
            Debug.Log(isSummoned);
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
            Debug.Log(BodyMesh.material.renderQueue);
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Fly;
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }

        public void SetHitJudgementObject(){ throw new System.NotImplementedException(); }
    }
}