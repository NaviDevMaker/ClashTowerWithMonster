using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using Game.Players.Sword;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;
using Game.Spells;


public interface IPlayer { }
namespace Game.Players
{
    ///<summary>
    ///Playerの基礎を集めたクラスで共通でつかうものを集めています。また、それぞれのStateに依存しない処理を集めています。
    ///</summary>
    public partial class PlayerControllerBase<T> : UnitBase, IPlayer where T : PlayerControllerBase<T>
    {
        [SerializeField] PlayerAnimatorPar animatorPar;
        [SerializeField] PlayerSkillData skillData;
        public PlayerSkillData SkillData => skillData;
        public Animator animator { get; private set; }

        public IdleStateBase<T> IdleState { get; protected set; }
        public MoveStateBase<T> MoveState { get; protected set; }
        public AttackStateBase<T> AttackState { get; protected set; }

        public DeathStateBase<T> DeathState { get; protected set; }

        public SkillStateBase<T> SpellState { get; protected set; }

        public CheckEnemyStateBase<T> CheckEnemyState { get; protected set; }
        public PlayerAnimatorPar AnimatorPar { get => animatorPar;}

        //public readonly float offsetY  = 1f;

        public StateMachineBase<T> currentState { get;private set; }
        public StateMachineBase<T> previousState { get; private set; } = null;

        public CancellationTokenSource cls = new CancellationTokenSource();

        public UnityAction<bool> OnAttackingPlayer;
        public UnityAction<bool> OnDeathPlayer;

        protected AddForceToUnit<PlayerControllerBase<T>> addForceToUnit;
        public AddForceToUnit<PushablePlayerSkillObj> addForceToUnit_Skill {get;set;}
        protected PushEffectUnit pushEffectUnit;
        public bool isUsingSkill { get; set; } = false;
        protected override void Start()
        {
            //Time.timeScale = 0.5f;//後で消して
            addForceToUnit = new AddForceToUnit<PlayerControllerBase<T>>(this, StatusData.PushAmount);
            animator = GetComponent<Animator>();
            base.Start();
            ChangeState(IdleState);
            CheckEnemyState?.OnEnter();
        }

        protected override void Update()
        {       
            Debug.Log(currentState);
            base.Update();
            SetPlayerHeight();
            if (isDead && currentState != DeathState) ChangeState(DeathState);

            if (!isUsingSkill && !isDead && InputManager.IsClickedSkillButton()) SpellState?.SkillInvoke();
            CheckEnemyState?.OnUpdate();
            currentState?.OnUpdate();
        }

        void FixedUpdate()
        {
            addForceToUnit.KeepDistance(moveType);
        }
        void SetPlayerHeight()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 10.0f, Vector3.down, out var hit, Mathf.Infinity, Layers.groundLayer))
            {
                var groundHeight = hit.point.y;
                var pos = new Vector3(transform.position.x, groundHeight, transform.position.z);
                transform.position = pos;
            }
        }
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return; // 実行中でなければ描画しない
            if (PlayerStatus != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position,PlayerStatus.AttackRange);
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * 100); // 長さ100で描画

            Gizmos.DrawRay(transform.position, transform.forward *2.0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, prioritizedRange);
        }
        public void ChangeState(StateMachineBase<T> nextState)
        {
            currentState?.OnExit();
            previousState = currentState;
            currentState = nextState;
            currentState.OnEnter();
        }
        public override void Damage(int damage)
        {
            base.Damage(damage);
        }

        public float GetAnimClipLength()
        {
            RuntimeAnimatorController runtimeAnimator = animator.runtimeAnimatorController;
            var clips = runtimeAnimator.animationClips;
            string currentStateName = null;
            switch (currentState)
            {

                case MoveStateBase<T>:
                    currentStateName = AnimatorPar.moveAnimClipName;
                    break;
                case AttackStateBase<T>:
                    currentStateName = AnimatorPar.attackAnimClipName;
                    break;
                case DeathStateBase<T>:
                    currentStateName = AnimatorPar.deathAnimClipName;
                    break;
                case IdleStateBase<T>:
                default:
                    break;
            }

            if (currentStateName != null)
            {
                foreach (var clip in clips)
                {
                    if (clip.name == currentStateName)
                    {
                        var length = clip.length;
                        return length;
                    }
                }
            }
            return 0;
        }

        public void AttackEvent()
        {
            AttackState.Attack_SimpleFromAnimEvent();
        }

        public void WaitIntervalEvent()
        {
            AttackState.WaitIntervalFromAnimEvent();
        }
    }
}

