using System.Collections.Generic;
using UnityEngine;

namespace Game.Monsters.DestructionMachine
{
    public class DestructionMachineController : MonsterControllerBase<DestructionMachineController>,ILongDistanceAttacker<DestructionMachineController>
    {
        //[SerializeField] Transform ryfulTransform;
        public int moverCount { get; private set; } = 5;
        Vector3 originalCannonBallPos;
        public List<LongDistanceAttack<DestructionMachineController>> movers { get; set; } = new List<LongDistanceAttack<DestructionMachineController>>();
        public Transform startTra { get;private set; }

        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//テスト用だから消して
        }
        //public int ID;//テスト用だから消してね
        protected override void Start()
        {
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
            base.Start();
        }

        public override void Initialize(int owner = -1)
        {
            moveType = MoveType.Walk;
            originalCannonBallPos = Vector3.zero;
            SetMoverToList();
            base.Initialize(owner);
            IdleState = new IdleState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
        }

        public void SetMoverToList()
        {
            var name = ProjectileAttackMonsterStatus.MoverStartTra.gameObject.name;
            var parentObj = this.gameObject.GetObject(name);
            if (parentObj == null) return;
            startTra = parentObj.transform;
            var localPos = originalCannonBallPos;
            var rot = Quaternion.identity;
            for (int i = 0; i < moverCount; i++)
            {
                var mover = Instantiate(ProjectileAttackMonsterStatus.Mover, Vector3.zero, Quaternion.identity);
                var moverCmp = mover.GetComponent<CannonBallMover>();
                moverCmp.Setup(this, startTra, localPos, rot,movers, SetToStartPos,ProjectileAttackMonsterStatus.ProjectileMoveSpeed);
            }
        }

        public void SetToStartPos(LongDistanceAttack<DestructionMachineController> cannonBall)
        {
            cannonBall.gameObject.SetActive(false);
            cannonBall.transform.SetParent(startTra);
            cannonBall.gameObject.transform.localPosition = originalCannonBallPos;
            cannonBall.IsReachedTargetPos = false;
            cannonBall.target = null;
        }
    }
}