using Game.Monsters.EvilMage;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monsters.Salamander
{
    public class SalamanderController : MonsterControllerBase<SalamanderController>
        ,ILongDistanceAttacker<SalamanderController>,IRepeatAttack,IRangeAttack
    {
        public List<LongDistanceAttack<SalamanderController>> movers { get; set; } 
            = new List<LongDistanceAttack<SalamanderController>>();
        public Transform startTra { get; private set; }
        public int moverCount => 20;
        public int repeatCount => 4;
        public GameObject rangeAttackObj { get; set; }

        protected override void Awake()
        {
            base.Awake();
            isSummoned = true;//テスト用だから消して
        }
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }
        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            base.Initialize(owner);
            SetMoverToList();
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }
        public async void EndMoveAction(LongDistanceAttack<SalamanderController> fireShot)
        {
            fireShot.IsReachedTargetPos = false;
            var cmp = fireShot.GetComponent<FireMover>();
            if(cmp != null)
            {
                var disapearTask = cmp.WaitEffectDissapaer();
                await disapearTask;
            }
            fireShot.gameObject.SetActive(false);
            fireShot.transform.SetParent(startTra);
            fireShot.gameObject.transform.localPosition = Vector3.zero; //ここSalamanderの攻撃時の口の部分位置になる
            fireShot.target = null;
            Debug.Log("初期化完了です");
        }
        public void SetMoverToList()
        {
            var name = ProjectileAttackMonsterStatus.MoverStartTra.gameObject.name;
            var parentObj = this.gameObject.GetObject(name);
            if (parentObj == null) return;
            startTra = parentObj.transform;
            var localPos = Vector3.zero;
            var rot = Quaternion.identity;
            for (int i = 0; i < moverCount; i++)
            {
                var mover = Instantiate(ProjectileAttackMonsterStatus.Mover, Vector3.zero, Quaternion.identity);
                var moverCmp = mover.GetComponent<FireMover>();
                moverCmp.Setup(this, startTra, localPos, rot, movers, EndMoveAction, ProjectileAttackMonsterStatus.ProjectileMoveSpeed);
            }
        }
        public void SetHitJudgementObject() { throw new System.NotImplementedException(); }
    }
}