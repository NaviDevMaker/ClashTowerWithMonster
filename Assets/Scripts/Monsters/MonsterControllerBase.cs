using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.HableCurve;

namespace Game.Monsters
{
    public class MonsterControllerBase<T> : UnitBase, IMonster where T : MonsterControllerBase<T>
    {
        [SerializeField] MonsterAnimatorPar monsterAnimPar;
        public Animator animator { get; private set; }
        public MonsterAnimatorPar MonsterAnimPar { get => monsterAnimPar; }

        public IdleStateBase<T> IdleState;
        public ChaseStateBase<T> ChaseState;
        public AttackStateBase<T> AttackState;
        public DeathStateBase<T> DeathState;
        StateMachineBase<T> currentState;
        AddForceToUnit<MonsterControllerBase<T>> addForceToUnit;
        protected override void Start()
        {
            base.Start();
            addForceToUnit = new AddForceToUnit<MonsterControllerBase<T>>(this);
            animator = GetComponent<Animator>();
            ChangeState(IdleState);
        }


        protected override void Update()
        {
            base.Update();
            if (isDead && currentState != DeathState)
            {
                ChangeToDeathState();
            }
            Debug.Log(currentState);
            currentState.OnUpdate();

        }

        private void FixedUpdate()
        {
            addForceToUnit.KeepDistance();
        }
        public virtual void ChangeState(StateMachineBase<T> nextState)
        {
            currentState?.OnExit();
            Debug.Log($"{currentState}��OnExit�ɂ͂���܂���");
            currentState = nextState;
            currentState.OnEnter();
            Debug.Log($"{currentState}��Onenter�ɂ͂���܂���");
        }
        //protected virtual void Initialize(int owner = -1){

        //   if(owner !=-1) SetMonsterSide(owner);
        //}

        //bool isMyMonster()
        //{
        //    return ownerID == 0;
        //}

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
                DrawEllipse(transform.position, radiusX, radiusZ, 32);
            }
        }

        void OnDrawGizmosSelected()
        {
           
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
    }
}

