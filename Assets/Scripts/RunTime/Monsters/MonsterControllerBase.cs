using Cysharp.Threading.Tasks;
using UnityEngine;
public interface IMonster 
{ 
    bool isSummonedInDeckChooseScene { get; set; }
    MonsterStatusData _MonsterStatus { get; }

    FlyingMonsterStatusData _FlyingMonsterStatus { get; }
    
    ProjectileAttackMonsterStatus _ProjectileAttackMonsterStatus { get; }

    RangeAttackMonsterStatusData _RangeAttackMonsterStatus { get; }
    FlyProjectileStatusData _FlyProjectileAttackMonsterStatus { get; }
    ContinuousAttackMonsterStatus _ContinuousAttackMonsterStatus { get;}
    UnitType _UnitType { get; }

    Renderer _BodyMesh { get; }
}

public interface IRangeAttack
{ 
    GameObject rangeAttackObj { get; set; }
    void SetHitJudgementObject();
}

public interface ISpecialIntervalActionInfo
{
    float actionInverval { get;}
   float elapsedTime { get; set; }
}
namespace Game.Monsters
{
    public class MonsterControllerBase<T> : UnitBase, IMonster,ISummonbable where T : MonsterControllerBase<T>
    {
        [SerializeField] MonsterAnimatorPar monsterAnimPar;
        public Animator animator { get; protected set;}
        public MonsterAnimatorPar MonsterAnimPar { get => monsterAnimPar; }
        public bool isSummoned { get; set; } = false;
        public string SummonedCardName { get; set; }
        public IdleStateBase<T> IdleState { get; protected set; }
        public ChaseStateBase<T> ChaseState { get; protected set; }
        public AttackStateBase<T> AttackState { get; protected set; }
        public DeathStateBase<T> DeathState { get; protected set; }
        protected StateMachineBase<T> currentState { get; private set;}
        public StateMachineBase<T> previusState { get; private set; }

        protected AddForceToUnit<MonsterControllerBase<T>> addForceToUnit;
        public bool isSummonedInDeckChooseScene { get; set; } = false;
        public MonsterStatusData _MonsterStatus => MonsterStatus;
        public FlyingMonsterStatusData _FlyingMonsterStatus => FyingMonsterStatus;
        public ProjectileAttackMonsterStatus _ProjectileAttackMonsterStatus => ProjectileAttackMonsterStatus;
        public RangeAttackMonsterStatusData _RangeAttackMonsterStatus => RangeAttackMonsterStatusData;
        public ContinuousAttackMonsterStatus _ContinuousAttackMonsterStatus => ContinuousAttackMonsterStatus;
        public FlyProjectileStatusData _FlyProjectileAttackMonsterStatus => FlyProjectileStatusData;

        public UnitType _UnitType => UnitType.monster;
        public Renderer _BodyMesh => BodyMesh;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
        }

        protected override void Start()
        {
            isSummoned = true;//����e�X�g�p����������R�����g�A�E�g����ĂȂ�����������Ă�
            Debug.Log("��������������������������������������������");
            base.Start();
            addForceToUnit = new AddForceToUnit<MonsterControllerBase<T>>(this, StatusData.PushAmount);
            ChangeState(IdleState);
            originalAnimatorSpeed = animator.speed;
        }
        protected override void Update()
        {
            if (!isSummonedInDeckChooseScene)
            {
                base.Update();
                Debug.Log($"{statusCondition.Freeze.isActive},{statusCondition.Freeze.isEffectedCount}");
                this.CheckFreeze_Unit(animator);
                this.CheckAbsorption();
                if (isSummoned)
                {
                    currentState?.OnUpdate();
                }
                if (isDead && currentState != DeathState)
                {
                    ChangeToDeathState();
                }
                //Debug.Log(currentState);
            }
            //����f�b�L�I���V�[���̎��Ɍ��{�p�̃����X�^�[�����̓s�x�폜���邩�炻�̂���
            else
            {
                if (isDead && currentState != DeathState)
                {
                    ChangeToDeathState();
                }
            }          
        }
        private void FixedUpdate()
        {
            if(isSummoned && IdleState.isEndSummon && !isDead) addForceToUnit.KeepDistance(moveType);
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
        protected void ChangeToDeathState()
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

