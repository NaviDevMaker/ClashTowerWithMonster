using Game.Monsters.DestructionMachine;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monsters.EvilMage
{
    public class EvilMageController : MonsterControllerBase<EvilMageController>,ILongDistanceAttacker<EvilMageController>
    {
        public List<LongDistanceAttack<EvilMageController>> movers {get;set;} = new List<LongDistanceAttack<EvilMageController>>();
        public int moverCount => 5;
        public Transform startTra { get;private set; }
        Vector3 orignalSpellStartPos = Vector3.zero;
        protected override void Awake()
        {
            base.Awake();
            //isSummoned = true;//テスト用だから消して
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
        public async void EndMoveAction(LongDistanceAttack<EvilMageController> mageSpellBall)
        {
            mageSpellBall.IsReachedTargetPos = false;
            var spellBall = mageSpellBall.transform.GetChild(1).gameObject;
            spellBall.SetActive(false);
            var cmp = mageSpellBall.GetComponent<MageSpellMover>();
            if (cmp != null)
            {
                var dissapearTask = cmp.WaitEffectDissapaer();
                await dissapearTask;
            }
            mageSpellBall.gameObject.SetActive(false);
            spellBall.SetActive(true);
            mageSpellBall.transform.SetParent(startTra);
            mageSpellBall.gameObject.transform.localPosition = Vector3.zero; //ここMageの攻撃時の手の部分位置になる
            mageSpellBall.target = null;
            Debug.Log("初期化完了です");
        }
        public void SetMoverToList()
        {
            var name = ProjectileAttackMonsterStatus.MoverStartTra.gameObject.name;
            var parentObj = this.gameObject.GetObject(name);
            if (parentObj == null) return;
            startTra = parentObj.transform;
            var localPos = orignalSpellStartPos;
            var rot = Quaternion.identity;
            for (int i = 0; i < moverCount; i++)
            {
                var mover = Instantiate(ProjectileAttackMonsterStatus.Mover, Vector3.zero, Quaternion.identity);
                var moverCmp = mover.GetComponent<MageSpellMover>();
                moverCmp.Setup(this, startTra, localPos, rot, movers, EndMoveAction,
                              ProjectileAttackMonsterStatus.ProjectileMoveSpeed,StatusData.AttackAmount);
            }
        }
    }
}