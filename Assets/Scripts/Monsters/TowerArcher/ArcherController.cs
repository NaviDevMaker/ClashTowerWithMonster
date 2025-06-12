using Game.Monsters.GuirdSlime;
using Game.Monsters.Slime;
using System.Collections.Generic;
using UnityEngine;


namespace Game.Monsters.Archer
{
    //ここに送られてくるtargetはタワーのほうであらかじめフィルターをかけられたものが送られてくるのでSideなどの比較はいらない
    public class ArcherController : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer myMesh;
        [SerializeField] Transform sampleArrowTransform;
        public Transform arrowHand;
        public Vector3 originalArrowPos { get; private set; }  = Vector3.zero;
        public Quaternion originalArrowRot { get; private set;} = Quaternion.identity;
        public GameObject arrow { get; set; }
        public Animator animator { get; private set; }
        public IdleState _IdleState { get; private set; }
        public BowShotState _BowShotState { get; private set; }
        public DeathState _DeathState { get; private set; }

        StateMachineBase<ArcherController> currentState;

        public readonly int shot = Animator.StringToHash("isShotting");
        public readonly int death = Animator.StringToHash("isDead");
        public UnitBase target { get; set; } = null;
        public float shotDuration { get; set; } = 0f;
        public SkinnedMeshRenderer MyMesh { get => myMesh;}
        public List<GunMover> shotGuns { get; set;}
        private void Start()
        {
            //Time.timeScale = 0.3f;
            animator = GetComponent<Animator>();
            Initialize();
            currentState = _IdleState;
            currentState.OnEnter();
        }
        public void ChangeState(StateMachineBase<ArcherController> nextState)
        {
            currentState?.OnExit();
            currentState = nextState;
            currentState?.OnEnter();
        }
        private void Update()
        {
            Debug.Log(currentState);
            currentState?.OnUpdate();
        }

        void Initialize()
        {
            _IdleState = new IdleState(this);
            _BowShotState = new BowShotState(this);
            _DeathState = new DeathState(this);

            originalArrowPos = sampleArrowTransform.localPosition;
            originalArrowRot = sampleArrowTransform.localRotation;
        }

        public float GetAnimClipLength()
        {
            RuntimeAnimatorController runtimeAnimator = animator.runtimeAnimatorController;
            var clips = runtimeAnimator.animationClips;

            string currentStateName = null;
            switch (currentState)
            {
                case IdleState:
                    break;
                case BowShotState:
                    currentStateName = "BowShot";
                    break;
                case DeathState:
                    currentStateName = "Death";
                    break;

            }

            
            if (currentStateName == null) return 0;

            if (currentStateName != null)
            {
                foreach (var clip in clips)
                {
                    if (clip.name == currentStateName)
                    {
                        var length = clip.length * animator.speed;
                        return length;
                    }
                }
            }
            return 0;
        }

        public void ActiveArrowEvent()
        {
            _BowShotState?.ActiveArrowFromAnimEvent();
        }

        public void ParentNullEvent()
        {
            _BowShotState?.SetparentToNull();
        }
        //悔しい
        public void BowShotEvent()
        {
            _BowShotState?.ShotToEnemyFromAnimEvent();
        }
    }

}

