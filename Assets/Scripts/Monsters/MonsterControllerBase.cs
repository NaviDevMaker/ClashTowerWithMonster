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
            Debug.Log("ｊｃｄさｃｄｓｈｋｃｓｄｊｄｓｃｓｄｋｓｄｎ");
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
            Debug.Log($"{currentState}がOnExitにはいりました");
            previusState = currentState != null? currentState : null;
            currentState = nextState;
            currentState.OnEnter();
            Debug.Log($"{currentState}がOnenterにはいりました");
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
            Debug.Log("死亡ステイトに変更します");
            ChangeState(DeathState);
        }
        private void OnDrawGizmos()
        {
            if (MonsterStatus != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, MonsterStatus.ChaseRange);

                Gizmos.color = Color.cyan;

                // 楕円をXZ平面に描画（前方を向いてるとは限らないので、簡易）
                DrawEllipse(transform.position,rangeX,rangeZ, 32);
            }
        }
        // 楕円描画関数
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
        //アニメーションのスピードをnormalizetime>= 1.0のときに0にしたいからそれをclipのeventにつける
        public void StopAnimation_AttackState()
        {
            AttackState.StopAnimFromEvent();
        }
    }
}

