using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Monsters.KingDragon
{
    public class KingDragonController : UnitBase, ITower, IBuilding, IInvincible
    {
        [SerializeField] List<TowerController> towerControllers;
        [SerializeField] KingDragonAnimPar kingDragonAnimPar;
        AddForceToUnit<KingDragonController> addForceToUnit;

        public KingDragonStatus KingDragonStatus => TowerStatus as KingDragonStatus;
        public StateMachineBase<KingDragonController> currentState { get; private set; }
        public StateMachineBase<KingDragonController> previousState { get; private set; } = null;
        public IdleState IdleState { get; private set; }
        public SearchState SearchState { get; private set; }
        public AttackState AttackState { get; private set; }
        public DeathState DeathState { get; private set; }

        public Animator animator { get; private set; }
        public bool IsInvincible { get; set; }
        public KingDragonAnimPar KingDragonAnimPar => kingDragonAnimPar;

        protected override void Start()
        {
            animator = GetComponent<Animator>();
            ChangeState(IdleState);
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }
        private void FixedUpdate()
        {
            addForceToUnit.KeepDistance(moveType);
        }
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
            IdleState = new IdleState(this);
            SearchState = new SearchState(this);
            AttackState = new AttackState(this);
            DeathState = new DeathState(this);
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

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, TowerStatus.SearchRadius);
        }
    }
}



