using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.KingDragon
{
    public class KingDragonController : UnitBase, ITower, IBuilding, IInvincible,IRepeatAttack,
                                        ILongDistanceAttacker<KingDragonController>
                                        
    {
        [SerializeField] List<TowerController> towerControllers;
        [SerializeField] Transform _startTra;
        [SerializeField] KingDragonAnimPar kingDragonAnimPar;
        AddForceToUnit<KingDragonController> addForceToUnit;

        public KingDragonStatus KingDragonStatus => StatusData as KingDragonStatus;
        public StateMachineBase<KingDragonController> currentState { get; private set; }
        public StateMachineBase<KingDragonController> previousState { get; private set; } = null;
        public IdleState IdleState { get; private set; }
        public SearchState SearchState { get; private set; }
        public AttackState AttackState { get; private set; }
        public DeathState DeathState { get; private set; }
        public KingDragonMethod kingDragonMethod { get; private set;} = null;
        public Animator animator { get; private set; }
        public bool IsInvincible { get; set; }
        public KingDragonAnimPar KingDragonAnimPar => kingDragonAnimPar;

        public int repeatCount => 10;

        public List<LongDistanceAttack<KingDragonController>> movers { get; set; }
               = new List<LongDistanceAttack<KingDragonController>>();

        public Transform startTra { get; private set;}

        public int moverCount => 10;

        protected override void Awake()
        {
            animator = GetComponent<Animator>();
            base.Awake();
        }
        protected override void Start()
        {
            ChangeState(IdleState);
            base.Start();
        }

        protected override void Update()
        {
            currentState?.OnUpdate();
            Debug.Log(currentState);
            base.Update();
            if (isDead && currentState != DeathState) ChangeToDeathState();
        }
        private void FixedUpdate() => addForceToUnit.KeepDistance(moveType);
        public void ChangeState(StateMachineBase<KingDragonController> nextState)
        {
            currentState?.OnExit();
            previousState = currentState;
            currentState = nextState;
            currentState.OnEnter();
        }
        public override void Initialize(int owner)
        {
            moveType = MoveType.Walk;
            addForceToUnit = new AddForceToUnit<KingDragonController>(this, StatusData.PushAmount);
            base.Initialize(owner);
            SetMoverToList();
            IdleState = new IdleState(this);
            SearchState = new SearchState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
            kingDragonMethod = new KingDragonMethod(this);
        }
        public bool IsDestroyedAllTower()
        {
            var isDestroyedEachTower = towerControllers.Select(tower =>
            {
                if (tower == null) return true;
                else return tower.isDead;
            });
            if (isDestroyedEachTower.Contains(false)) return false;
            else return true;
        }
        public void StopAnimation_AttackState() => AttackState.StopAnimFromEvent();
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, KingDragonStatus.SearchRadius);
        }
        public async void EndMoveAction(LongDistanceAttack<KingDragonController> kingDraFireShot)
        {
            kingDraFireShot.IsReachedTargetPos = false;
            var cmp = kingDraFireShot.GetComponent<KingDragonFireMover>();
            if (cmp != null)
            {
                var disapearTask = cmp.WaitEffectDissapaer();
                await disapearTask;
            }
            kingDraFireShot.gameObject.SetActive(false);
            kingDraFireShot.transform.SetParent(startTra);
            kingDraFireShot.gameObject.transform.localPosition = Vector3.zero; //ここSalamanderの攻撃時の口の部分位置になる
            kingDraFireShot.target = null;
            Debug.Log("初期化完了です");
        }
        public void SetMoverToList()
        {
            startTra = _startTra;
            var localPos = Vector3.zero;
            var rot = Quaternion.identity;
            var moveSpeed = KingDragonStatus.MoverSpeed;
            var oriAttackAmount = KingDragonStatus.AttackAmount;
            var adjustDenominator = 3f;
            var quantity = oriAttackAmount - Mathf.RoundToInt(oriAttackAmount / adjustDenominator);
            var targetAmount = Mathf.RoundToInt(quantity / (float)repeatCount);

            Debug.Log(targetAmount);
            for (int i = 0; i < moverCount; i++)
            {
                var mover = Instantiate(KingDragonStatus.KingDragonFireMover, Vector3.zero, Quaternion.identity);
                var moverCmp = mover.GetComponent<KingDragonFireMover>();
                moverCmp.Setup(this, startTra, localPos, rot, movers,EndMoveAction,moveSpeed, targetAmount);
            }
        }
        void ChangeToDeathState()
        {
            Debug.Log("死亡ステイトに変更します");
            ChangeState(DeathState);
        }
    }
}



