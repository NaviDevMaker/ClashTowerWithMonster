using Cysharp.Threading.Tasks;
using Game.Monsters.BigEyeMonster;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.HableCurve;

public interface IMonster { }
namespace Game.Monsters
{

    public class MonsterControllerBase<T> : UnitBase, IMonster,ISummonbable where T : MonsterControllerBase<T>
    {
        [SerializeField] MonsterAnimatorPar monsterAnimPar;
        public Animator animator { get; protected set;}
        public MonsterAnimatorPar MonsterAnimPar { get => monsterAnimPar; }
        public bool isSummoned { get; set; } = false;

        public IdleStateBase<T> IdleState;
        public ChaseStateBase<T> ChaseState;
        public AttackStateBase<T> AttackState;
        public DeathStateBase<T> DeathState;
        protected StateMachineBase<T> currentState { get; private set;}
        public StateMachineBase<T> previusState { get; private set; }
        protected AddForceToUnit<MonsterControllerBase<T>> addForceToUnit;
        protected override void Start()
        {
            Debug.Log("��������������������������������������������");
            base.Start();
            addForceToUnit = new AddForceToUnit<MonsterControllerBase<T>>(this, StatusData.PushAmount);
            animator = GetComponent<Animator>();
            ChangeState(IdleState);
        }
        protected override void Update()
        {
            base.Update();
            if (isSummoned)
            {
                currentState?.OnUpdate();
            }
            if (isDead && currentState != DeathState)
            {
                ChangeToDeathState();
            }
            Debug.Log(currentState);
          
        }

        private void FixedUpdate()
        {
            if(isSummoned && IdleState.isEndSummon) addForceToUnit.KeepDistance(moveType);
        }
        public virtual void ChangeState(StateMachineBase<T> nextState)
        {
            currentState?.OnExit();
            Debug.Log($"{currentState}��OnExit�ɂ͂���܂���");
            previusState = currentState != null? currentState : null;
            currentState = nextState;
            currentState.OnEnter();
            Debug.Log($"{currentState}��Onenter�ɂ͂���܂���");
        }

        public float GetAnimClipLength()
        {
            RuntimeAnimatorController runtimeAnimator = animator.runtimeAnimatorController;
            var clips = runtimeAnimator.animationClips;
            string currentStateName = null;
            switch (currentState)
            {

                case ChaseStateBase<T>:
                    currentStateName = monsterAnimPar.chaseAnimClipName;
                    break;
                case AttackStateBase<T>:
                    currentStateName = monsterAnimPar.attackAnimClipName;
                    break;
                case DeathStateBase<T>:
                    currentStateName = monsterAnimPar.deathAnimClipName;
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

        void ChangeToDeathState()
        {
            Debug.Log("���S�X�e�C�g�ɕύX���܂�");
            ChangeState(DeathState);
        }
        private void OnDrawGizmos()
        {
            if (MonsterStatus != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, MonsterStatus.ChaseRange);

                Gizmos.color = Color.cyan;

                // �ȉ~��XZ���ʂɕ`��i�O���������Ă�Ƃ͌���Ȃ��̂ŁA�ȈՁj
                DrawEllipse(transform.position,rangeX,rangeZ, 32);
            }
        }
        // �ȉ~�`��֐�
        void DrawEllipse(Vector3 center, float radiusX, float radiusZ, int segments)
        {
            Vector3 prevPoint = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                float x = Mathf.Cos(angle) * radiusX;
                float z = Mathf.Sin(angle) * radiusZ;
                Vector3 nextPoint = new Vector3(x, 0, z) + center;

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, nextPoint);
                }
                prevPoint = nextPoint;
            }
        }
        //�A�j���[�V�����̃X�s�[�h��normalizetime>= 1.0�̂Ƃ���0�ɂ��������炻���clip��event�ɂ���
        public void StopAnimation_AttackState()
        {
            AttackState.StopAnimFromEvent();
        }
    }
}

